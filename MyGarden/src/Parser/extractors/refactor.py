import json

def transform_json(data):
    # Проходим по каждому объекту в списке
    for item in data:
        # Проходим по каждому ключу в объекте
        for key in item:
            # Если значение по ключу является списком
            if isinstance(item[key], list):
                # Создаем новый словарь для объединения свойств
                new_dict = {}
                # Проходим по каждому элементу в списке
                for sub_item in item[key]:
                    # Добавляем каждое свойство в новый словарь
                    new_dict.update(sub_item)
                # Заменяем список на новый словарь
                item[key] = new_dict
    return data

def remove_empty_values(data):
    """
    Рекурсивно удаляет свойства с пустыми значениями (null, {}, []).
    """
    if isinstance(data, dict):
        # Удаляем пустые значения из словаря
        return {k: remove_empty_values(v) for k, v in data.items() if v not in [None, {}, []]}
    elif isinstance(data, list):
        # Удаляем пустые значения из списка
        return [remove_empty_values(item) for item in data if item not in [None, {}, []]]
    else:
        # Возвращаем значение, если это не словарь и не список
        return data


# Чтение JSON-файла
with open('C:/Users/timofey.latypov/Documents/MyGarden-server/MyGarden/src/Parser/catalogues/stroy_podskazka/data/plant_data.json', 'r', encoding='utf-8') as file:
    data = json.load(file)

# Преобразование данных
transformed_data = remove_empty_values(transform_json(data))

# Запись преобразованных данных в новый JSON-файл
with open('output.json', 'w', encoding='utf-8') as file:
    json.dump(transformed_data, file, ensure_ascii=False, indent=4)

print("Преобразование завершено. Результат сохранен в файл 'output.json'.")