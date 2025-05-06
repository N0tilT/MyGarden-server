import pytesseract
from PIL import Image,ImageEnhance
from recognizer.similarity import *
import numpy as np
from pytesseract import Output
import cv2
import os

def identify(images,data,top_number = 5):
    identifiers = []
    text = [prepare_text(extract_text(x)) for x in data]
    for item in images:
        image = cv2.imread(item)
        q = pytesseract.image_to_string(image,lang="rus+eng")
        recognized_text = prepare_text(q)

        tfidf_similarities = calculate_tfidf_similarity(recognized_text, text)
        levenshtein_similarities = calculate_levenshtein_similarity(recognized_text, text)
        
        top_indices_tfidf = np.argsort(tfidf_similarities)[-top_number:][::-1]
        top_indices_levenshtein = np.argsort(levenshtein_similarities)[-top_number:][::-1]

        cur_ids = []
        for i in range(len(top_indices_tfidf)):
            most_similarity_percentage = max(tfidf_similarities[top_indices_tfidf[i]],
                                            levenshtein_similarities[top_indices_levenshtein[i]])
            index = top_indices_tfidf[i] if most_similarity_percentage == top_indices_tfidf[i] else top_indices_levenshtein[i]
            cur_ids.append((data[index]['id'],data[index]['title'],data[index]['description'],most_similarity_percentage))
        identifiers.append((item,cur_ids))
    return identifiers

def prepare_text(recognized):
    return recognized.replace('\n', ' ').replace('\t', ' ').replace(' ','').replace('.','').replace(',','').replace(':','').replace('-','').lower()