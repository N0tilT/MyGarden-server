import json
from collections import Counter

def get_property_paths(data, parent_key='', separator='/'):
    items = []
    if isinstance(data, dict):
        for key, value in data.items():
            new_key = f"{parent_key}{separator}{key}" if parent_key else key
            items.append(new_key)
            if isinstance(value, (dict, list)):
                items.extend(get_property_paths(value, new_key, separator))
    elif isinstance(data, list):
        for item in data:
            items.extend(get_property_paths(item, parent_key, separator))
    return items

def get_top_level_properties(data):
    if isinstance(data, list):
        # Если данные — это список, обрабатываем каждый элемент
        top_level_keys = set()
        for item in data:
            if isinstance(item, dict):
                top_level_keys.update(item.keys())
        return list(top_level_keys)
    elif isinstance(data, dict):
        # Если данные — это словарь, возвращаем его ключи
        return list(data.keys())
    else:
        # Если данные не являются списком или словарем, возвращаем пустой список
        return []
    
with open('D:/Prog/MyGarden-server/MyGarden/src/Parser/catalogues/stroy_podskazka/data/summarized.json', 'r', encoding='utf-8') as file:
    data = json.load(file)

print("Анализ записей растений")
total_elements = len(data)
print("Количество записей:",total_elements)
property_paths = get_property_paths(data)
top_level_property_paths = get_top_level_properties(data)

print("Общее количество уникальных свойств:",len(set(property_paths)))
counter = Counter(property_paths)
threshold = 20
print("Нижняя граница количества вхождений свойства (в %):",threshold)
sorted_counts = [(item,count,f'{percentage:.2f}%') for item,count,percentage in [(item, count, (count / total_elements) * 100) for item, count in counter.most_common()] if percentage>threshold and len(item.split('/'))>1]
print("Отобранные свойства:\n",sorted_counts)
print("Количество отобранных свойств:",len(sorted_counts))

with open('D:/Prog/MyGarden-server/MyGarden/src/Parser/catalogues/stroy_podskazka/data/summarized.json', 'r', encoding='utf-8') as file:
    data = json.load(file)

print("Анализ записей цветов")
total_elements = len(data)
print("Количество записей:",total_elements)
property_paths = get_property_paths(data)
top_level_property_paths = get_top_level_properties(data)

print("Общее количество уникальных свойств:",len(set(property_paths)))
threshold = 20
print("Нижняя граница количества вхождений свойства (в %):",threshold)
counter = Counter(property_paths)
sorted_counts = [(item,count,f'{percentage:.2f}%') for item,count,percentage in [(item, count, (count / total_elements) * 100) for item, count in counter.most_common()] if percentage>threshold and len(item.split('/'))>1]
print("Отобранные свойства:\n",sorted_counts)
print("Количество отобранных свойств:",len(sorted_counts))