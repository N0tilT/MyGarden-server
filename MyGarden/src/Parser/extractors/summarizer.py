import json
import re
import ijson 
import torch
import nltk
from transformers import pipeline
from nltk import sent_tokenize

try:
    sent_tokenize("test")
except LookupError:
    print("Downloading NLTK 'punkt' resources...")
    nltk.download("punkt", quiet=True)
    print("Downloading NLTK 'punkt_tab' resources...")
    nltk.download("punkt_tab", quiet=True)
    print("NLTK resources downloaded!")

summarizer = pipeline(
    "summarization",
    model="IlyaGusev/rut5_base_sum_gazeta",
    device=0 if torch.cuda.is_available() else -1
)

def split_text(text, max_chunk_length=500):
    sentences = sent_tokenize(text, language="russian")
    chunks = []
    current_chunk = []
    current_length = 0

    for sent in sentences:
        sent_length = len(sent.split())
        if current_length + sent_length > max_chunk_length and current_chunk:
            chunks.append(" ".join(current_chunk))
            current_chunk = []
            current_length = 0
        current_chunk.append(sent)
        current_length += sent_length

    print(len(chunks))
    if current_chunk:
        chunks.append(" ".join(current_chunk))
    return chunks

def process_large_text(text, max_input_length=1024, max_summary_length=150):
    if len(text.split()) <= max_input_length:
        return process_plant_data(text, max_summary_length)
    
    chunks = split_text(text)
    print(len(chunks))
    combined_summary = []
    
    for chunk in chunks:
        chunk_summary = process_plant_data(chunk, max_summary_length)
        combined_summary.append(chunk_summary)
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
    
    return " ".join(combined_summary)

def clean_text(text):
    return re.sub(r'\s+[^\s]*$', '', text.strip())

def process_plant_data(text,max_length=150):
    input_length = len(text.split())
    if input_length < 15:
        return text
    
    max_length = min(max_length, int(input_length * 0.7))  
    min_length = max(int(input_length * 0.2), 10)         
    
    
    with torch.no_grad():  
        summary = summarizer(
            text,
            max_length=max_length,
            min_length=min_length,
            do_sample=False,
            truncation=True,
            early_stopping=True,
            no_repeat_ngram_size=3 
        )[0]['summary_text']
    return summary

def process_text(text):
    parts = list(set(part.strip().lower() for part in text.split('|||') if part.strip()))
    combined = " ".join(parts)
    if len(combined) > 500: 
        return process_large_text(combined)
    
    x = process_plant_data(combined).split()
    return clean_text(" ".join(list(dict.fromkeys(x))))

def process_data(plant):
    processed_plant = {}
    for key, value in plant.items():
        if key in ("id", "link"):
            processed_plant[key] = value
        elif isinstance(value, dict):
            processed_plant[key] = process_data(value)
        else:
            text = process_text(value)
            print(text)
            processed_plant[key] = text
    return processed_plant

def main():
    
    with open("D:/Prog/MyGarden-server/MyGarden/src/Parser/catalogues/stroy_podskazka/data/merged_flowers.json", "r", encoding="utf-8") as f, \
         open('D:/Prog/MyGarden-server/MyGarden/src/Parser/catalogues/stroy_podskazka/data/summarized_flowers.json', 'w', encoding='utf-8') as out_file:

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
            print(f"Processed plant {count}", end='\r')
            
            del processed_plant
            if torch.cuda.is_available():
                torch.cuda.empty_cache()

        out_file.write('\n]')
        print(f"\nCompleted processing {count} plants")

if __name__ == "__main__":
    main()