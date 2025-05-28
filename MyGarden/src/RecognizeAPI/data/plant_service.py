import json
import sqlite3
from utils.utils import deduplicateData

with open('./plant_classified_formatted.json', 'r', encoding='utf-8') as file:
    plant_classification_data = json.load(file)
with open('./plant_articles_formatted.json', 'r', encoding='utf-8') as file:
    plant_articles_data = json.load(file)
with open('./flower_classified_formatted.json', 'r', encoding='utf-8') as file:
    flower_classification_data = json.load(file)
with open('./flower_articles_formatted.json', 'r', encoding='utf-8') as file:
    flower_articles_data = json.load(file)


def deduplicatePlantsData(catalogues):
    for catalog in catalogues:
        deduplicateData(f'./catalogues/{catalog}')


def get_plants_data():
    conn = sqlite3.connect('./data/plants.db')
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM plants")
    rows = cursor.fetchall()
    cursor.close()
    conn.close()
    return [{'id': row[0], 'title': row[2], 'description': row[3]}for row in rows]


def get_plant_classification():
    return []


def get_plant_articles():
    return []


def get_plant_description():
    return []
