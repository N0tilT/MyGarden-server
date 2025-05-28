import json
from collections import defaultdict


def deep_merge(source, overrides):
    """Рекурсивно объединяет два словаря, сохраняя вложенные структуры."""
    merged = source.copy()
    for key, value in overrides.items():
        if isinstance(value, dict) and isinstance(merged.get(key), dict):
            merged[key] = deep_merge(merged[key], value)
        else:
            merged[key] = (str(merged[key]) + "|||" + str(value) if str(merged[key]) != str(value) else value) if key in merged.keys(
            ) and not isinstance(merged.get(key), dict) and not isinstance(value, dict) else value
    return merged


with open('../catalogues/stroy_podskazka/data/translated_flowers.json', 'r', encoding='utf-8') as file:
    data = json.load(file)

groups = defaultdict(list)
non_grouped_objs = []

for obj in data:
    link = obj.get('link', '')
    parts = link.strip('/').split('/')
    if len(parts) >= 2:
        group_key = parts[0]
        groups[group_key].append(obj)
    else:
        non_grouped_objs.append(obj)

result = []
for group_objs in groups.values():
    merged_obj = {}
    for obj in group_objs:
        merged_obj = deep_merge(merged_obj, obj)
    result.append(merged_obj)

result.extend(non_grouped_objs)


with open('merged_flowers2.json', 'w', encoding='utf-8') as file:
    json.dump(result, file, ensure_ascii=False, indent=4)
