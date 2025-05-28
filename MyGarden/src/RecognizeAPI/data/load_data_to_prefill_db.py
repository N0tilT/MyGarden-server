import os
import json
import sqlite3

# with open('./plant_classified_formatted.json', 'r', encoding='utf-8') as file:
#     plant_classification_data = json.load(file)
# with open('./plant_articles_formatted.json', 'r', encoding='utf-8') as file:
#     plant_articles_data = json.load(file)
# with open('./plant_summary_formatted.json', 'r', encoding='utf-8') as file:
#     plant_summary_data = json.load(file)
# with open('./flower_classified_formatted.json', 'r', encoding='utf-8') as file:
#     flower_classification_data = json.load(file)
# with open('./flower_articles_formatted.json', 'r', encoding='utf-8') as file:
#     flower_articles_data = json.load(file)
# with open('./flower_summary_formatted.json', 'r', encoding='utf-8') as file:
#     flower_summary_data = json.load(file)
with open('./flower_types.json', 'r', encoding='utf-8') as file:
    flower_types = json.load(file)
with open('./plant_types.json', 'r', encoding='utf-8') as file:
    plant_types = json.load(file)


def create_tables(conn):
    with conn:
        conn.execute('''
        CREATE TABLE IF NOT EXISTS plant_type (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT
        )''')
        conn.execute('''
        CREATE TABLE IF NOT EXISTS plant_prefill (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            plant_type_id INTEGER,
            type TEXT,
            articles TEXT,
            labels TEXT,
            summary TEXT,
            FOREIGN KEY (plant_type_id) REFERENCES plant_type(id)
        )''')
        conn.execute('''
        CREATE TABLE IF NOT EXISTS flower_type (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT
        )''')
        conn.execute('''
        CREATE TABLE IF NOT EXISTS flower_prefill (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            plant_type_id INTEGER,
            type TEXT,
            articles TEXT,
            labels TEXT,
            summary TEXT,
            FOREIGN KEY (plant_type_id) REFERENCES plant_type(id)
        )''')


def load_data(classified_path, articles_path, summary_path):
    with open(classified_path, 'r', encoding='utf-8') as f:
        classified = json.load(f)
    with open(articles_path, 'r', encoding='utf-8') as f:
        articles = json.load(f)
    with open(summary_path, 'r', encoding='utf-8') as f:
        summary = json.load(f)
    return classified, articles, summary


def insert_plants(conn, classified, articles_map, summary_data):
    cursor = conn.cursor()

    for plant in classified:
        type_id = plant['plant_type']

        plant_type_id = 0
        plant_type_title = ""
        for type in plant_types:
            if type['id'] == type_id:
                plant_type_id = type['id']
                plant_type_id = type['title']

        article = next(
            (item for item in articles_map if item["plant_type"] == type_id),
            {}
        )
        summary = next(
            (item['summary']
             for item in summary_data if item["plant_type"] == type_id),
            {}
        )
        articles_json = json.dumps(article, ensure_ascii=False)
        labels = json.dumps(plant['labels'], ensure_ascii=False)

        # Insert into plant_prefill
        cursor.execute('''
            INSERT INTO plant_prefill 
            (plant_type_id, type, articles, labels, summary)
            VALUES (?, ?, ?, ?, ?)
        ''', (plant_type_id, plant_type_title, articles_json, labels, summary))


def insert_flowers(conn, classified, articles_map, summary_data):
    cursor = conn.cursor()

    for plant in classified:
        type_id = plant['plant_type']

        plant_type_id = 0
        plant_type_title = ""
        for type in flower_types:
            if type['id'] == type_id:
                plant_type_id = type['id']
                plant_type_id = type['title']
        # Find matching article
        article = next(
            (item for item in articles_map if item["plant_type"] == type_id),
            {}  # Default if not found
        )
        summary = next(
            (item['summary']
             for item in summary_data if item["plant_type"] == type_id),
            {}
        )
        articles_json = json.dumps(article, ensure_ascii=False)
        labels = json.dumps(plant['labels'], ensure_ascii=False)

        cursor.execute('''
            INSERT INTO flower_prefill 
            (plant_type_id, type, articles, labels, summary)
            VALUES (?, ?, ?, ?, ?)
        ''', (plant_type_id, plant_type_title, articles_json, labels, summary))


def main():
    conn = sqlite3.connect('./prefill.db')
    create_tables(conn)

    plant_classified, plant_articles = load_data(
        './plant_classified_formatted.json',
        './plant_articles_formatted.json',
        './plant_summary_formatted.json'
    )
    insert_plants(conn, plant_classified, plant_articles)

    flower_classified, flower_articles = load_data(
        './flower_classified_formatted.json',
        './flower_articles_formatted.json',
        './flower_summary_formatted.json'
    )
    insert_flowers(conn, flower_classified, flower_articles)

    conn.commit()
    conn.close()


if __name__ == '__main__':
    main()
