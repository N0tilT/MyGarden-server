import os
import json
import sqlite3

with open('./plant_classified_formatted.json', 'r', encoding='utf-8') as file:
    plant_classification_data = json.load(file)
with open('./plant_articles_formatted.json', 'r', encoding='utf-8') as file:
    plant_articles_data = json.load(file)
with open('./flower_classified_formatted.json', 'r', encoding='utf-8') as file:
    flower_classification_data = json.load(file)
with open('./flower_articles_formatted.json', 'r', encoding='utf-8') as file:
    flower_articles_data = json.load(file)


def create_tables(conn):
    with conn:
        conn.execute('''
        CREATE TABLE IF NOT EXISTS plant_type (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT
        )''')
        conn.execute('''
        CREATE TABLE IF NOT EXISTS prefill (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            plant_type_id INTEGER,
            title jsonb,
            description jsonb,
            FOREIGN KEY(plant_type_id) REFERENCES catalplant_typeogues(id)
        )''')


def insert_data(conn, catalog_title, plants):
    cursor = conn.cursor()
    cursor.execute("INSERT INTO catalogues (title) VALUES (?)",
                   (catalog_title,))
    manufacturer_id = cursor.lastrowid

    for plant in plants:
        cursor.execute("INSERT INTO plants (catalog_id, title, description) VALUES (?, ?, ?)",
                       (manufacturer_id, plant['title'], plant['description']))


def main(json_directory, db_path):
    conn = sqlite3.connect(db_path)
    create_tables(conn)

    for catalog_folder in os.listdir(json_directory):
        catalog_path = os.path.join(json_directory, catalog_folder)
        catalog_path = os.path.join(catalog_path, 'data')

        if os.path.isdir(catalog_path):
            plants = []
            for json_file in os.listdir(catalog_path):
                if json_file.endswith('.json'):
                    with open(os.path.join(catalog_path, json_file), 'r', encoding='utf-8') as file:
                        data = json.load(file)
                        plants.extend(data)

            insert_data(conn, catalog_folder, plants)

    conn.commit()
    conn.close()


main('catalogues', './data/plants.db')
