import json
from collections import defaultdict

with open('../catalogues/stroy_podskazka/data/sum_plants_completed.json', 'r', encoding='utf-8') as file:
    data = json.load(file)
with open('../catalogues/stroy_podskazka/data/plant_types.json', 'r', encoding='utf-8') as file:
    plant_types = json.load(file)

result = []
for obj in data:
    type_name = obj['link'].strip("/").split("/")[0]
    for type in plant_types:
        if type['title'] == type_name:
            result.append({
                "plant_type": type['id'],
                "summary": obj["summary"]
            })
            break


with open('plant_summary_formatted.json', 'w', encoding='utf-8') as file:
    json.dump(result, file, ensure_ascii=False, indent=4)
