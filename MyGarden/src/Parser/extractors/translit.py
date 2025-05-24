def transliterate(text):
    translit_dict = {
        'а': 'a', 'б': 'b', 'в': 'v', 'г': 'g', 'д': 'd', 'е': 'e',
        'ё': 'yo', 'ж': 'zh', 'з': 'z', 'и': 'i', 'й': 'j', 'к': 'k',
        'л': 'l', 'м': 'm', 'н': 'n', 'о': 'o', 'п': 'p', 'р': 'r',
        'с': 's', 'т': 't', 'у': 'u', 'ф': 'f', 'х': 'kh', 'ц': 'ts',
        'ч': 'ch', 'ш': 'sh', 'щ': 'shch', 'ъ': '', 'ы': 'y', 'ь': '',
        'э': 'e', 'ю': 'yu', 'я': 'ya'
    }
    return ''.join([translit_dict.get(c, c) for c in text.lower()])

def reverse_transliterate(text):
    reverse_dict = {
        'shch': 'щ', 'kh': 'х', 'ts': 'ц', 'ch': 'ч', 'sh': 'ш',
        'yu': 'ю', 'ya': 'я', 'yo': 'ё', 'zh': 'ж', 'a': 'а', 'b': 'б',
        'v': 'в', 'g': 'г', 'd': 'д', 'e': 'е', 'z': 'з', 'i': 'и',
        'j': 'й', 'k': 'к', 'l': 'л', 'm': 'м', 'n': 'н', 'o': 'о',
        'p': 'п', 'r': 'р', 's': 'с', 't': 'т', 'u': 'у', 'f': 'ф',
        'y': 'ы'
    }
    
    result = []
    i = 0
    while i < len(text):
        for length in [4, 3, 2, 1]:
            if i + length > len(text):
                continue
            substr = text[i:i+length].lower()
            if substr in reverse_dict:
                result.append(reverse_dict[substr])
                i += length
                break
        else:
            result.append(text[i])
            i += 1
    return ''.join(result)


data = 'C:/Users/timofey.latypov/Documents/MyGarden-server/MyGarden/src/Parser/catalogues/stroy_podskazka/links/plant_links_data.txt'
plant_links = []
with open(data,'r') as links_file:
    plant_links = links_file.read().split(',\n')

plant_links = set([link.split("/")[1] for link in plant_links])
for link in plant_links:
    print(reverse_transliterate(link))