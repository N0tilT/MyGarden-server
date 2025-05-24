import json
import joblib
import re
from natasha import (
    Doc,
    Segmenter,
    MorphVocab,
    NewsEmbedding,
    NewsMorphTagger,
    NewsNERTagger,
    DatesExtractor
)

segmenter = Segmenter()
morph_vocab = MorphVocab()
emb = NewsEmbedding()
morph_tagger = NewsMorphTagger(emb)
ner_tagger = NewsNERTagger(emb)
dates_extractor = DatesExtractor(morph_vocab)

RUSSIAN_STOPWORDS = {
    'и', 'в', 'во', 'не', 'что', 'он', 'на', 'я', 'с', 'со', 'как', 'а',
    'то', 'все', 'она', 'так', 'его', 'но', 'да', 'ты', 'к', 'у', 'же',
    'вы', 'за', 'бы', 'по', 'только', 'ее', 'мне', 'было', 'вот', 'от',
    'меня', 'еще', 'нет', 'о', 'из', 'ему', 'теперь', 'когда', 'даже',
    'ну', 'вдруг', 'ли', 'если', 'уже', 'или', 'ни', 'быть', 'был', 'него',
    'до', 'вас', 'нибудь', 'опять', 'уж', 'вам', 'ведь', 'там', 'потом',
    'себя', 'ничего', 'ей', 'может', 'они', 'тут', 'где', 'есть', 'надо',
    'ней', 'для', 'мы', 'тебя', 'их', 'чем', 'была', 'сам', 'чтоб', 'без',
    'будто', 'чего', 'раз', 'тоже', 'себе', 'под', 'будет', 'ж', 'тогда',
    'кто', 'этот', 'того', 'потому', 'этого', 'какой', 'совсем', 'ним',
    'здесь', 'этом', 'один', 'почти', 'мой', 'тем', 'чтобы', 'нее', 'сейчас',
    'были', 'куда', 'зачем', 'всех', 'никогда', 'можно', 'при', 'наконец',
    'два', 'об', 'другой', 'хоть', 'после', 'над', 'больше', 'тот', 'через',
    'эти', 'нас', 'про', 'всего', 'них', 'какая', 'много', 'разве', 'три',
    'эту', 'моя', 'впрочем', 'хорошо', 'свою', 'этой', 'перед', 'иногда',
    'лучше', 'чуть', 'том', 'нельзя', 'такой', 'им', 'более', 'всегда',
    'конечно', 'всю', 'между'
}
MONTHS = {
    'январ': 'январь',
    'феврал': 'февраль',
    'март': 'март',
    'апрел': 'апрель',
    'май': 'май',
    'июн': 'июнь',
    'июл': 'июль',
    'август': 'август',
    'сентябр': 'сентябрь',
    'октябр': 'октябрь',
    'ноябр': 'ноябрь',
    'декабр': 'декабрь'
}

def normalize_month_name(month_str):
    """Нормализация названий месяцев с учетом всех падежей"""
    month_lower = month_str.lower()
    # Ищем наиболее длинное совпадение начала слова
    for prefix, full_name in MONTHS.items():
        if month_lower.startswith(prefix):
            return full_name.capitalize()
    return month_str.capitalize()

def extract_ripening_period(text):
    doc = Doc(text)
    doc.segment(segmenter)
    doc.tag_ner(ner_tagger)
    
    valid_periods = []
    
    time_patterns = [
        (r'\b(\d+)\s*-\s*(\d+)\s+(дн|день|дней|суток)\b', 'days'),
        (r'\b(с|по|до|от)\s+(\d+\s+[а-я]+)', 'month_range'),
        (r'\b(январ|феврал|март|апрел|ма[йя]|июн|июл|август|сентябр|октябр|ноябр|декабр)[ьья]*\b', 'month'),
        (r'\b(весна|лето|осень|зима)\b', 'season')
    ]
    
    for pattern, ptype in time_patterns:
        for match in re.finditer(pattern, text, re.IGNORECASE):
            full_match = match.group()
            if ptype == 'days' and 10 <= int(match.group(1)) <= 365:
                valid_periods.append(f"Дни: {match.group(1)}-{match.group(2)}")
            elif ptype == 'month':
                # Исправлено: нормализация названия месяца
                month_name = normalize_month_name(match.group(1))
                valid_periods.append(f"Месяц: {month_name}")
            elif ptype == 'season':
                valid_periods.append(f"Сезон: {match.group(1).capitalize()}")

    dates_extractor = DatesExtractor(morph_vocab)
    matches = dates_extractor(text)
    for match in matches:
        if match.fact:
            date_str = format_date_fact(match.fact)
            if date_str:
                valid_periods.append(date_str)

    filtered_periods = []
    for period in valid_periods:
        if not re.search(r'\b(устойчивость|форма|цвет|сорт|вид|тип)\b', period, re.I):
            filtered_periods.append(period)
    
    if filtered_periods:
        for period in filtered_periods:
            if 'Дни:' in period:
                return period.split(': ')[1]
        return filtered_periods[0]
    
    return None

def format_date_fact(fact):
    """Обработка объектов дат из Natasha"""
    try:
        # Для диапазонов дат
        if hasattr(fact, 'start') and hasattr(fact, 'end'):
            start_month = normalize_month_name(fact.start.month.name) if fact.start.month else None
            end_month = normalize_month_name(fact.end.month.name) if fact.end.month else None
            if start_month and end_month:
                return f"{start_month}-{end_month}"
            start = get_date_part(fact.start)
            end = get_date_part(fact.end)
            if start and end:
                return f"{start}-{end}"
        
        # Для одиночных дат
        elif hasattr(fact, 'day') or hasattr(fact, 'month') or hasattr(fact, 'year'):
            return get_date_part(fact)
        
    except AttributeError as e:
        print(f"Ошибка обработки даты: {e}")
    
    return None

def get_date_part(date):
    """Извлечение частей даты с проверкой"""
    parts = []
    
    # Проверяем наличие атрибутов
    if hasattr(date, 'day') and date.day is not None:
        parts.append(f"{date.day:02d}")
    if hasattr(date, 'month') and date.month is not None:
        parts.append(f"{date.month:02d}")
    if hasattr(date, 'year') and date.year is not None:
        parts.append(str(date.year))
    
    return '-'.join(parts) if parts else None

def preprocess_text(text):
    doc = Doc(text.lower().strip())
    doc.segment(segmenter)
    doc.tag_morph(morph_tagger)
    
    tokens = []
    for token in doc.tokens:
        token.lemmatize(morph_vocab)
        lemma = token.lemma
        if (
            lemma.lower() not in RUSSIAN_STOPWORDS and
            lemma.isalpha() and
            len(lemma) > 2
        ):
            tokens.append(lemma)
    
    return ' '.join(tokens)

watering_model = joblib.load('./natasha/watering_model.pkl')
light_model = joblib.load('./natasha/light_model.pkl')
fertilizer_model = joblib.load('./natasha/fertilizer_model.pkl')

watering_encoder = joblib.load('./natasha/watering_encoder.pkl')
light_encoder = joblib.load('./natasha/light_encoder.pkl')
fertilizer_encoder = joblib.load('./natasha/fertilizer_encoder.pkl')

def predict(text):
    processed_text = preprocess_text(text)
    ripening_period = extract_ripening_period(text)
    
    watering_pred = watering_model.predict([processed_text])
    light_pred = light_model.predict([processed_text])
    fertilizer_pred = fertilizer_model.predict([processed_text])
    return {
        'WateringNeed': watering_encoder.inverse_transform(watering_pred)[0],
        'LightNeed': light_encoder.inverse_transform(light_pred)[0],
        'Fertilizer': list(fertilizer_encoder.inverse_transform(fertilizer_pred)[0]),
        'RipeningPeriod': ripening_period
    }

def flatten_section(data):
    items = []
    if isinstance(data, dict):
        for k, v in data.items():
            if isinstance(v, dict):
                items.append(flatten_section(v))
            elif isinstance(v, str) and k not in ["id","link"]:
                items.append(f"{k}: {v.replace('|||', ' ')}")
            elif isinstance(v, list):
                items.append(", ".join([item.replace("|||", " ") for item in v]))
    return ". ".join(items)

def process_file(input_file, output_file):
    with open(input_file, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    for item in data:
        sections_text = flatten_section(item)
        
        predictions = predict(sections_text)
        
        item['predicted_labels'] = predictions
    
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

if __name__ == '__main__':
    input_json = "..\\catalogues\\stroy_podskazka\\data\\summarized_plants.json"
    output_json = "..\\catalogues\\stroy_podskazka\\data\\natasha_plants.json"
    process_file(input_json, output_json)
    print(f"Обработка завершена. Результаты сохранены в {output_json}")
    input_json = "..\\catalogues\\stroy_podskazka\\data\\summarized_flowers.json"
    output_json = "..\\catalogues\\stroy_podskazka\\data\\natasha_flowers.json"
    process_file(input_json, output_json)
    print(f"Обработка завершена. Результаты сохранены в {output_json}")