import sqlite3
from utils.utils import deduplicateData

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
    return [{'title':row[2],'description':row[3]}for row in rows]