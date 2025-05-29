import re
import json
import joblib
import ijson
import os
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.multiclass import OneVsRestClassifier
from sklearn.multioutput import MultiOutputClassifier
from sklearn.svm import SVC
from sklearn.pipeline import Pipeline
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
from sklearn.preprocessing import MultiLabelBinarizer
import shutil


data = json.load(open("..\\catalogues\\stroy_podskazka\\data\\prepared.json", "r", encoding="utf-8"))
for idx, plant in enumerate(data):
    if "labels" not in plant:
        print(f"Ошибка: Растение {idx} не имеет раздела 'labels'!")
    elif "WateringNeed" not in plant["labels"]:
        print(f"Ошибка: Растение {idx} не имеет метки 'WateringNeed'!")

CLASSIFICATION_CONFIG = {
    "target_labels": {
        "WateringNeed": ["Редкий полив", "Регулярный полив", "Постоянная влажность"],
        "LightNeed": ["Теневыносливое", "Полутень", "Яркое солнце"]
    },
    "section_weights": {
        "Выращивание": 3.0,
        "Основные характеристики": 2.0,
        "Что это такое?": 1.5,
        "*": 1.0
    },
    "model_params": {
        "tfidf": {
            "max_features": None,  # Убрать ограничение
            "ngram_range": (1, 3), # Расширить диапазон n-грамм
            "stop_words": None     # Не игнорировать стоп-слова
        },
        "classifier": {
            "C": 1.0,
            "kernel": "linear"
        }
    }
}
def flatten_section(data):
    items = []
    if isinstance(data, dict):
        for k, v in data.items():
            if isinstance(v, dict):
                items.append(flatten_section(v))
            elif isinstance(v, str):
                items.append(f"{k}: {v.replace('|||', ' ')}")
            elif isinstance(v, list):
                items.append(", ".join([item.replace("|||", " ") for item in v]))
    return ". ".join(items)

def load_prepared_data(filename):
    """Загрузка подготовленных данных"""
    with open(filename, 'r', encoding='utf-8') as f:
        data = json.load(f)
    return data

def preprocess_plant(plant, section_weights):
    """Структурированная предобработка данных"""
    weighted_text = []
    
    for section, content in plant["sections"].items():
        weight = section_weights.get(section, section_weights["*"])           
        section_text = flatten_section(content)
        
        weighted_text.extend([section_text] * int(weight))
    
    return " ".join(weighted_text)


def prepare_training_data(data):
    X, y = [], {"WateringNeed": [], "LightNeed": [], "Fertilizer": []}
    
    for plant in data:
        # Проверка наличия ключа 'labels'
        if "labels" not in plant:
            raise ValueError("Отсутствует раздел 'labels' в данных растения!")
        
        # Проверка наличия метки WateringNeed
        if "WateringNeed" not in plant["labels"]:
            raise ValueError(f"Растение {plant['id']} не имеет метки 'WateringNeed'!")
        fert_labels = plant["labels"].get("Fertilizer", [])
        y["Fertilizer"].append(fert_labels if isinstance(fert_labels, list) else [])
    
    mlb = MultiLabelBinarizer()
    y["Fertilizer"] = mlb.fit_transform(y["Fertilizer"])
    
    if len(mlb.classes_) < 2:
        mlb.fit([["минеральные"], ["органические"]])
    
    for target in ["WateringNeed", "LightNeed"]:
        if not any(y[target]):
            raise ValueError(f"Нет данных для {target}. Проверьте метки в prepared.json!")
        
    return X, y, mlb

def train_models(X, y):
    """Обучение моделей для каждой цели"""
    models = {}
    for target in ["WateringNeed", "LightNeed"]:
        X_filtered, y_filtered = [], []
        for x_item, y_item in zip(X, y[target]):
            if y_item is not None:
                X_filtered.append(x_item)
                y_filtered.append(y_item)
        unique_classes = np.unique(y_filtered)
        if len(unique_classes) < 2:
            print(f"Недостаточно данных для обучения {target}. Найдено классов: {len(unique_classes)}")
            continue
        try:
            model = Pipeline([
                ('tfidf', TfidfVectorizer(
                    max_features=CLASSIFICATION_CONFIG["model_params"]["tfidf"]["max_features"],
                    ngram_range=CLASSIFICATION_CONFIG["model_params"]["tfidf"]["ngram_range"]
                )),
                    ('clf', LogisticRegression() )
            ])
            model.fit(X, y["Fertilizer"])
            models["Fertilizer"] = (model, mlb)
        except ValueError as e:
            print(f"Ошибка обучения удобрений: {str(e)}")
            models["Fertilizer"] = (None, mlb)
    mlb = y.pop("mlb", None)
    if mlb:
        if len(mlb.classes_) < 2:
            print(f"Недостаточно классов удобрений. Найдено: {len(mlb.classes_)}")
        else:
            model = Pipeline([
                ('tfidf', TfidfVectorizer(
                    max_features=CLASSIFICATION_CONFIG["model_params"]["tfidf"]["max_features"],
                    ngram_range=CLASSIFICATION_CONFIG["model_params"]["tfidf"]["ngram_range"]
                )),
                ('clf', MultiOutputClassifier(
                    SVC(
                        C=CLASSIFICATION_CONFIG["model_params"]["classifier"]["C"],
                        kernel=CLASSIFICATION_CONFIG["model_params"]["classifier"]["kernel"],
                        probability=True
                    )
                ))
            ])
            try:
                model.fit(X, y["Fertilizer"])
                models["Fertilizer"] = (model, mlb)
            except ValueError as e:
                print(f"Ошибка обучения модели удобрений: {str(e)}")
    return models

def save_models(models, path="trained_models"):
    """Сохранение моделей"""
    os.makedirs(path, exist_ok=True)
    for name, model in models.items():
        if name == "Fertilizer":
            joblib.dump(model[0], os.path.join(path, "fertilizer_clf.pkl"))
            joblib.dump(model[1], os.path.join(path, "fertilizer_mlb.pkl"))
        else:
            joblib.dump(model, os.path.join(path, f"{name}_clf.pkl"))

def load_models(path="trained_models"):
    """Загрузка моделей"""
    models = {}
    if os.path.exists(os.path.join(path, "fertilizer_clf.pkl")):
        models["Fertilizer"] = (
            joblib.load(os.path.join(path, "fertilizer_clf.pkl")),
            joblib.load(os.path.join(path, "fertilizer_mlb.pkl"))
        )
    for target in ["WateringNeed", "LightNeed"]:
        model_path = os.path.join(path, f"{target}_clf.pkl")
        if os.path.exists(model_path):
            models[target] = joblib.load(model_path)
    return models

def predict_plant(plant_data, models):
    """Предсказание для нового растения"""
    processed_text = preprocess_plant(
        {"sections": plant_data},
        CLASSIFICATION_CONFIG["section_weights"]
    )
    
    result = {"confidence": {}}
    
    for target in ["WateringNeed", "LightNeed"]:
        if target in models:
            proba = models[target].predict_proba([processed_text])[0]
            max_idx = np.argmax(proba)
            result[target] = models[target].classes_[max_idx]
            result["confidence"][target] = float(proba[max_idx])
    
    if "Fertilizer" in models:
        model, mlb = models["Fertilizer"]
        if model is not None:
            try:
                proba = model.predict_proba([processed_text])
                result["Fertilizer"] = mlb.inverse_transform((proba > 0.5).astype(int))[0]
            except ValueError:
                result["Fertilizer"] = ["не определено"]
    
    result["RipeningPeriod"] = extract_ripening_period(processed_text)
    
    return result



def extract_ripening_period(text):
    patterns = [
        (r"(Период от всходов до уборки:?\s*)(\d+\s*-\s*\d+[\s-]*(?:дней|дня|день|месяцев))", 2),
        (r"(созревания[\D]*)(\d+\s*-\s*\d+\s*(?:дн|дней|дня|месяцев))", 2),
        (r"(\d+\s*-\s*\d+)\s*дней? к созреванию", 1),
        (r"(созревает за|срок созревания)[\s:]*(\d+\s*-\s*\d+|\d+)\s*(?:дн|дней|дня|месяцев)", 2),
        (r"(техническая спелость)[\D]*(\d+\+?\s*дней)", 2),
        (r"(сроки созревания|созревание):?\s*([а-яё]+[- ]?[а-яё]+)", 2),
        (r"(раннеспелый|среднеспелый|позднеспелый|скороспелый)", 1)
    ]

    matches = []
    for pattern, group in patterns:
        for match in re.finditer(pattern, text, re.IGNORECASE):
            try:
                period = match.group(group).strip()
                if period not in matches:
                    matches.append(period)
            except IndexError:
                continue
    
    priority_order = [
        (r'\d+\s*-\s*\d+', 0),    
        (r'\d+', 0),              
        (r'дней|дня|день', 0),    
        (r'ранне|средне|поздне', 0) 
    ]

    for pattern, _ in priority_order:
        for match in matches:
            if re.search(pattern, match):
                return match
    
    return "Не указано" if not matches else matches[0]

def detect_toxicity(text):
    return bool(re.search(r"ядовит|токсич|опасн|не съедоб", text, re.IGNORECASE))

def extract_fertilizer_info(text):
    fertilizers = {
        "органические": [r"компост", r"навоз", r"перегной"],
        "минеральные": [r"азотные", r"фосфорные", r"калийные", r"минеральные"],
        "комплексные": [r"NPK", r"комплексные"]
    }
    found = []
    for fert_type, patterns in fertilizers.items():
        for pattern in patterns:
            if re.search(pattern, text, re.IGNORECASE):
                found.append(fert_type)
                break
    return found if found else ["Не указано"]

def main():
    prepared_data = load_prepared_data("..\\catalogues\\stroy_podskazka\\data\\prepared.json")
    X, y, mlb = prepare_training_data(prepared_data)
    y["mlb"] = mlb
    models = train_models(X, y)
    save_models(models)
    
    loaded_models = load_models()
    with open("..\\catalogues\\stroy_podskazka\\data\\summarized_plants.json", "r", encoding="utf-8") as f, \
         open("..\\catalogues\\stroy_podskazka\\data\\classified_plants.json", 'w', encoding='utf-8') as out_file:

        out_file.write('[\n')
        first_entry = True
        count = 0

        for plant in ijson.items(f, 'item'):
            processed_plant = predict_plant(plant,models=loaded_models)
            processed_plant["id"] = plant["id"]
            processed_plant["link"] = plant["link"].split("|||")[0].strip("/").split("/")[0]
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

        out_file.write('\n]')
        print(f"\nCompleted processing {count} plants")
        
if __name__ == "__main__":
    if os.path.exists("trained_models"):
        shutil.rmtree("trained_models")
    main()