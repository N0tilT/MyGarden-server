import asyncio
import ijson
import json
from googletrans import Translator

def split_text(text, max_length=500):
    """Разбивает текст на чанки, стараясь не разрывать слова."""
    chunks = []
    while len(text) > max_length:
        split_at = text.rfind(' ', 0, max_length)
        if split_at == -1:
            split_at = max_length  
        chunks.append(text[:split_at])
        text = text[split_at:].lstrip()  
    chunks.append(text)
    return chunks

translator = Translator()
async def translate_to(text,language = "en"):
    """Переводит текст на английский, разбивая на чанки."""
    print("All:",len(text))
    chunks = split_text(text)
    translated_chunks = []
    
    for chunk in chunks:
        try:
            print("Source chunk:",len(chunk))
            translated = await translator.translate(chunk, dest=language)
            translated_chunks.append(translated.text)
            print("Translated chunk:",len(translated.text))
        except Exception as e:
            print(f"Ошибка перевода: {e}")
            translated_chunks.append(chunk)
    
    print("Translated:",len(" ".join(translated_chunks)))
    return " ".join(translated_chunks)

async def main():
    with open("../catalogues/stroy_podskazka/data/merged_flowers.json", "r", encoding="utf-8") as f, \
         open('../catalogues/stroy_podskazka/data/translated_flowers.json', 'w', encoding='utf-8') as out_file:
        
        out_file.write('[\n')
        first_entry = True

        for plant in ijson.items(f, 'item'):
            texts = []
            
            def extract_texts(data):
                for k, v in data.items():
                    if k in ("id", "link"):
                        continue
                    if isinstance(v, dict):
                        extract_texts(v)
                    elif isinstance(v, str):
                        texts.append(v.replace("|||"," "))
            
            extract_texts(plant)
            combined_text = " ".join(texts)
            
            try:
                translated_summary = await translate_to_english(combined_text)
            except Exception as e:
                print(f"Ошибка при обработке растения {plant.get('id')}: {e}")
                translated_summary = "Translation error"

            processed_plant = {
                "id": plant.get("id"),
                "link": plant.get("link"),
                "summary": translated_summary
            }
            
            plant_json = json.dumps(processed_plant, ensure_ascii=False, indent=4)
            formatted_entry = '\n'.join(plant_json.split('\n'))
            
            if not first_entry:
                out_file.write(',\n')
            out_file.write(f'    {formatted_entry}')
            
            first_entry = False

        out_file.write('\n]')

if __name__ == "__main__":
    asyncio.run(main())