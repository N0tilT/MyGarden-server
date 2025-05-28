import asyncio
import ijson
import json
import aiohttp
import re
from aiohttp import ClientTimeout
from translate import translate_to

async def process_plant(summary, max_retries=3):
    
    url = 'http://127.0.0.1:1234/v1/chat/completions'
    headers = {'Content-Type': 'application/json'}
    
    # Паттерны для очистки текста
    cleanup_patterns = [
        (r'\{\s*«Резюме»:\s*«?', ''),  # Удаление шаблонных фраз
        (r'»?\s*\}', ''),
        (r'(?i)(резюме|описание|характеристики)[:\s]*', ''),
        (r'\s+', ' '),  # Удаление лишних пробелов
    ]

    def clean_text(text):
        """Постобработка полученного текста"""
        for pattern, replacement in cleanup_patterns:
            text = re.sub(pattern, replacement, text)
        return text.strip()

    for attempt in range(max_retries):
        try:
            async with aiohttp.ClientSession() as session:
                async with session.post(url, headers=headers, json={
                    "model": "gemma-3-4b-it-qat",
                    "messages": [{"role": "user", "content": summary}],
                    "temperature": 0.7,
                    "max_tokens": -1,
                    "stream": False
                },timeout=ClientTimeout(total=120)) as response:
                    
                    if response.status != 200:
                        continue

                    response_json = await response.json()
                    content = response_json['choices'][0]['message']['content']
                    if(len(content)<10):
                        continue
                    extract = json.loads(content)
                    cleaned_content = clean_text(await translate_to(extract['summary'],"ru"))
                    print(cleaned_content)

                    return cleaned_content
                    
        except Exception as e:
            print(f"Attempt {attempt+1} failed: {str(e)}")
        
        await asyncio.sleep(1)  # Задержка между попытками

    return "Failed to generate proper summary"  # Фолбек при неудаче

async def main():
    with open("../catalogues/stroy_podskazka/data/translated_plants.json", "r", encoding="utf-8") as f, \
         open('../catalogues/stroy_podskazka/data/sum_plants.json', 'w', encoding='utf-8') as out_file:
        
        out_file.write('[\n')
        first_entry = True

        for plant in ijson.items(f, 'item'):
            summary = plant.get('summary', '')
            
            processed_summary = await process_plant(summary,10)
            
            processed_plant = {
                "id": plant.get("id"),
                "link": plant.get("link"),
                "summary": processed_summary
            }
            
            plant_json = json.dumps(processed_plant, ensure_ascii=False, indent=4)
            if not first_entry:
                out_file.write(',\n')
            out_file.write(f'    {plant_json}')
            first_entry = False

        out_file.write('\n]')

if __name__ == "__main__":
    asyncio.run(main())