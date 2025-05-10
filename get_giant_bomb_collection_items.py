import requests
import json

# Base URL for the IA advanced search API
base_url = "https://archive.org/advancedsearch.php"

# Query parameters
params = {
    "q": "collection:giant-bomb-audio",
    "fl[]": "identifier",
    "rows": 100,
    "output": "json"
}

# The total number of items in the collection
total_items = 4451
page = 1
all_identifiers = []

# Loop through the collection in chunks
while (page * 100) <= (total_items + 100):
    params['page'] = page
    response = requests.get(base_url, params=params)
    data = response.json()

    # Collect the item identifiers
    for doc in data['response']['docs']:
        all_identifiers.append(doc['identifier'])

    # Move to the next chunk
    page += 1

# Save the list of identifiers to a file
with open('giant_bomb_audio_identifiers.json', 'w') as f:
    json.dump(all_identifiers, f)
