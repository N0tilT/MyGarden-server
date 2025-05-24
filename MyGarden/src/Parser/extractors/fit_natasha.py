import json
import joblib
from natasha import (
    Doc,
    Segmenter,
    MorphVocab,
    NewsEmbedding,
    NewsMorphTagger
)
from sklearn.pipeline import Pipeline
from sklearn.svm import SVC
from sklearn.multiclass import OneVsRestClassifier
from sklearn.preprocessing import MultiLabelBinarizer, LabelEncoder
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression

segmenter = Segmenter()
morph_vocab = MorphVocab()
emb = NewsEmbedding()
morph_tagger = NewsMorphTagger(emb)

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

def preprocess_text(text):
    doc = Doc(text.lower().strip())
    doc.segment(segmenter)
    doc.tag_morph(morph_tagger)
    
    tokens = []
    for token in doc.tokens:
        token.lemmatize(morph_vocab)
        lemma = token.lemma
        if (
            lemma.lower() not in RUSSIAN_STOPWORDS
            and lemma.isalpha()
            and len(lemma) > 2
        ):
            tokens.append(lemma)
    
    return ' '.join(tokens)


with open("..\\catalogues\\stroy_podskazka\\data\\prepared.json", 'r', encoding='utf-8') as f:
    data = json.load(f)

texts = []
watering_labels = []
light_labels = []
fertilizer_labels = []

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

for item in data:
    texts.append(preprocess_text(flatten_section(item)))
    
    labels = item['labels']
    print(labels)
    watering_labels.append(labels['WateringNeed'])
    light_labels.append(labels['LightNeed'])
    fertilizer_labels.append(labels['Fertilizer'])

watering_encoder = LabelEncoder()
light_encoder = LabelEncoder()
fertilizer_encoder = MultiLabelBinarizer()

watering_encoded = watering_encoder.fit_transform(watering_labels)
light_encoded = light_encoder.fit_transform(light_labels)
fertilizer_encoded = fertilizer_encoder.fit_transform(fertilizer_labels)

watering_model = Pipeline([
    ('tfidf', TfidfVectorizer(
        ngram_range=(1, 2),
        min_df=2,
        max_df=0.9
    )),
    ('clf', SVC(
        kernel='rbf',
        C=1.5,
        class_weight='balanced'
    ))
])

light_model = Pipeline([
    ('tfidf', TfidfVectorizer(
        ngram_range=(1, 3),
        max_features=500
    )),
    ('clf', LogisticRegression(
        max_iter=1000,
        class_weight='balanced',
        C=0.8
    ))
])

fertilizer_model = Pipeline([
    ('tfidf', TfidfVectorizer(
        ngram_range=(1, 2),
        min_df=3
    )),
    ('clf', OneVsRestClassifier(
        LogisticRegression(
            max_iter=1000,
            class_weight='balanced'
        ),
        n_jobs=-1
    ))
])

watering_model.fit(texts, watering_encoded)
light_model.fit(texts, light_encoded)
fertilizer_model.fit(texts, fertilizer_encoded)

joblib.dump(watering_model, './natasha/watering_model.pkl')
joblib.dump(light_model, './natasha/light_model.pkl')
joblib.dump(fertilizer_model, './natasha/fertilizer_model.pkl')
joblib.dump(watering_encoder, './natasha/watering_encoder.pkl')
joblib.dump(light_encoder, './natasha/light_encoder.pkl')
joblib.dump(fertilizer_encoder, './natasha/fertilizer_encoder.pkl')
