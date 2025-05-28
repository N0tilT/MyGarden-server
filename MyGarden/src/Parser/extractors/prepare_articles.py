import json
from collections import defaultdict

with open('../catalogues/stroy_podskazka/data/flower_articles.json', 'r', encoding='utf-8') as file:
    data = json.load(file)
with open('../catalogues/stroy_podskazka/data/flower_types.json', 'r', encoding='utf-8') as file:
    plant_types = json.load(file)

result = []
for obj in data:
    obj_type = obj['type']
    for type in plant_types:
        if type['title'] == obj_type:
            result.append({
                "id": obj["id"],
                "type": obj["type"],
                "plant_type": type['id'],
                "articles": obj["articles"]
            })
            break


with open('formatted_articles.json', 'w', encoding='utf-8') as file:
    json.dump(result, file, ensure_ascii=False, indent=4)
