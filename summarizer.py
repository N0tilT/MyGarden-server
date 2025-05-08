import json
import re
import torch
from transformers import pipeline

summarizer = pipeline("summarization", model="IlyaGusev/rut5_base_sum_gazeta")

def clean_text(text):
    return re.sub(r'\s+[^\s]*$', '', text.strip())

def process_plant_data(text):
    summary = summarizer(
        text,
        do_sample=False,
        truncation=True,
        early_stopping=True,
    )[0]['summary_text']
    return summary

def process_text(text):
    parts=list(set(part.strip().lower() for part in text.split('|||') if part.strip()))
    combined = " ".join(parts) 
    x = process_plant_data(combined).split()   
    return clean_text(" ".join(list(dict.fromkeys(x)))) 

def process_data(plant):
    for key,value in plant.items():
        if not(key == "id" or key == "link"):
            if isinstance(value,dict):
                process_data(value)
            else:
                plant[key] = process_text(plant[key])
                print(plant[key])

with open("output.json", "r", encoding="utf-8") as f:
    data = json.load(f)
    
for plant in data:
    process_data(plant)

with open('summarized.json', 'w', encoding='utf-8') as file:
    json.dump(data, file, ensure_ascii=False, indent=4)