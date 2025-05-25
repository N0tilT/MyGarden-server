import json
import re
import ijson 
import torch
import nltk
import logging
from transformers import AutoTokenizer
from nltk import sent_tokenize
from optimum.onnxruntime import ORTModelForSeq2SeqLM

# Configure logging
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[logging.StreamHandler()]
)

try:
    sent_tokenize("test")
except LookupError:
    logging.info("Downloading NLTK punkt resources...")
    nltk.download("punkt", quiet=True)
    nltk.download("punkt_tab", quiet=True)

CONFIG = {
    "max_input_length": 1024,
    "max_summary_length": 150,
    "min_summary_length": 40,
    "num_beams": 2, 
    "chunk_size": 500,
    "execution_provider": "CPUExecutionProvider"
}

logging.info("Initializing tokenizer and model...")
tokenizer = AutoTokenizer.from_pretrained("IlyaGusev/rut5_base_sum_gazeta")
logging.info("Tokenizer initialized")

logging.info("Loading summarization model with %s...", CONFIG["execution_provider"])
summarizer = ORTModelForSeq2SeqLM.from_pretrained(
    "IlyaGusev/rut5_base_sum_gazeta",
    provider=CONFIG["execution_provider"],
    use_cache=False
)
logging.info("Summarization model loaded successfully")

def split_text(text):
    logging.debug("Splitting text into chunks...")
    sentences = sent_tokenize(text, language="russian")
    chunks, current_chunk, current_length = [], [], 0

    for sent in sentences:
        sent_length = len(sent.split())
        if current_length + sent_length > CONFIG["chunk_size"] and current_chunk:
            chunks.append(" ".join(current_chunk))
            current_chunk, current_length = [], 0
        current_chunk.append(sent)
        current_length += sent_length

    if current_chunk:
        chunks.append(" ".join(current_chunk))
    logging.info("Split text into %d chunks", len(chunks))
    return chunks

def postprocess_summary(text):
    logging.debug("Postprocessing summary...")
    text = re.sub(r'(?<=[.])\s+(?=[А-Я])', ' ', text)
    sentences = re.split(r'(?<=[.!?])\s+', text)
    seen = set()
    unique = []
    
    for sent in sentences:
        key = re.sub(r'\W+', '', sent).lower()
        if key not in seen and 15 < len(sent) < 500:
            seen.add(key)
            unique.append(sent)
    
    logging.debug("Filtered from %d to %d sentences", len(sentences), len(unique))
    return ' '.join(unique).strip()

def process_large_text(text):
    logging.info("Processing large text with batch method")
    chunks = split_text(text)
    if not chunks:
        return ""

    logging.debug("Tokenizing %d chunks...", len(chunks))
    inputs = tokenizer(
        chunks,
        max_length=CONFIG["max_input_length"],
        truncation=True,
        padding=True,
        return_tensors="pt"
    )

    with torch.inference_mode():
        logging.debug("Generating summaries for batch...")
        summary_ids = summarizer.generate(
            **inputs,
            max_length=CONFIG["max_summary_length"],
            min_length=CONFIG["min_summary_length"],
            num_beams=CONFIG["num_beams"],
            early_stopping=True
        )

    return " ".join(tokenizer.batch_decode(summary_ids, skip_special_tokens=True))

def clean_text(text):
    cleaned = re.sub(r'\s+[^\s]*$', '', text.strip())
    logging.debug("Cleaned text: %.50s...", cleaned)
    return cleaned

def process_plant_data(text):
    logging.info("Processing plant data with direct method")
    input_length = len(text.split())
    if input_length < 15:
        return text

    dynamic_max_length = min(CONFIG["max_summary_length"], int(input_length * 0.7))
    logging.debug("Using dynamic max length: %d", dynamic_max_length)
    
    inputs = tokenizer(
        text,
        max_length=CONFIG["max_input_length"],
        truncation=True,
        padding="max_length",
        return_tensors="pt"
    )

    with torch.inference_mode():
        logging.debug("Generating summary...")
        summary_ids = summarizer.generate(
            **inputs,
            max_length=dynamic_max_length,
            min_length=CONFIG["min_summary_length"],
            num_beams=CONFIG["num_beams"],
            early_stopping=True
        )

    return tokenizer.decode(summary_ids[0], skip_special_tokens=True)

def process_text(text):
    logging.info("Processing combined text")
    parts = list(set(part.strip().lower() for part in text.split('|||') if part.strip()))
    combined = " ".join(parts)
    
    if len(combined) > 500: 
        logging.info("Using large text processing (length: %d)", len(combined))
        processed = process_large_text(combined)
    else:
        logging.info("Using plant data processing (length: %d)", len(combined))
        processed = process_plant_data(combined)
    
    x = processed.split()
    return postprocess_summary(clean_text(" ".join(list(dict.fromkeys(x)))))

def process_data(plant):
    logging.debug("Processing plant data structure")
    texts = []
    
    def extract_texts(data):
        for k,v in data.items():
            if k in ("id", "link"):
                continue
            elif isinstance(v, dict):
                extract_texts(v)
            elif isinstance(v, str):
                texts.append(v)
    
    extract_texts(plant)
    logging.info("Extracted %d text fragments", len(texts))
    combined_text = " ".join(texts)
    
    return {
            "id": plant.get("id"),
            "link": plant.get("link"),
            "summary": process_text(combined_text)
        }

def main():
    logging.info("Starting main processing")
    with open("../catalogues/stroy_podskazka/data/merged_flowers.json", "r", encoding="utf-8") as f, \
         open('../catalogues/stroy_podskazka/data/summarized_flowers33.json', 'a+', encoding='utf-8') as out_file:
        
        out_file.write('[\n')
        first_entry = True
        count = 0

        for plant in ijson.items(f, 'item'):
            processed_plant = process_data(plant)
            plant_json = json.dumps(processed_plant, ensure_ascii=False, indent=4)
            formatted_entry = '\n'.join(f'    {line}' for line in plant_json.split('\n'))
            
            if first_entry:
                first_entry = False
            else:
                out_file.write(',\n')
            
            out_file.write(formatted_entry)
            count += 1
            logging.info("Processed plant %d (ID: %s)", count, processed_plant.get("id", "unknown"))
            
            del processed_plant
            if torch.cuda.is_available():
                torch.cuda.empty_cache()

        out_file.write('\n]')
        logging.info("Completed processing %d plants", count)

if __name__ == "__main__":
    main()