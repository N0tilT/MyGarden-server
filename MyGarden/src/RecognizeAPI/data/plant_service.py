import json
import sqlite3
import difflib
from utils.translit import transliterate

def get_plants_data():
    conn = sqlite3.connect('./data/plants.db')
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM plants")
    rows = cursor.fetchall()
    cursor.close()
    conn.close()
    return [{'id': row[0], 'title': row[2], 'description': row[3]}for row in rows]

def get_prefill_data(type):
    transliterated_name = transliterate(type)
    
    conn = sqlite3.connect('./data/prefill.db')
    cursor = conn.cursor()
    
    try:
        cursor.execute("SELECT id, title FROM type")
        plant_types = cursor.fetchall()
        
        if not plant_types:
            return None
            
        type_names = [plant[1] for plant in plant_types]
        
        matches = difflib.get_close_matches(
            transliterated_name, 
            type_names, 
            n=1,
            cutoff=0.6  
        )
        
        if not matches:
            return None
            
        best_match = matches[0]
        
        type_id = next((plant[0] for plant in plant_types if plant[1] == best_match), None)
        
        if not type_id:
            return None
            
        cursor.execute(
            "SELECT summary, labels, articles FROM prefill WHERE type_id = ?", 
            (type_id,)
        )
        prefill_data = cursor.fetchone()
        
        if not prefill_data:
            return None
            
        summary, labels_json, articles_json = prefill_data
        
        labels = json.loads(labels_json) if labels_json else {}
        articles = json.loads(articles_json) if articles_json else []
        
        return {
            "type": type,
            "summary": summary,
            "labels": labels,
            "articles": articles
        }
        
    finally:
        cursor.close()
        conn.close()
    