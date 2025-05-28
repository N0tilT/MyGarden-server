import json
from collections import defaultdict

with open('../catalogues/stroy_podskazka/data/classified_flowers.json', 'r', encoding='utf-8') as file:
    data = json.load(file)
with open('../catalogues/stroy_podskazka/data/flower_types.json', 'r', encoding='utf-8') as file:
    plant_types = json.load(file)

result = []
for obj in data:
    type_name = obj['link'].strip("/").split("/")[0]
    for type in plant_types:
        if type['title'] == type_name:
            result.append({
                "plant_type": type['id'],
                "labels": obj["predicted_labels"]
            })
            break


with open('formatted_classification.json', 'w', encoding='utf-8') as file:
    json.dump(result, file, ensure_ascii=False, indent=4)
