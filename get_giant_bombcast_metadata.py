import requests
import json

# Base URL for the IA metadata endpoint
base_url = "https://archive.org/metadata/"

# Load the list of identifiers from your previously saved JSON file
with open('giant_bomb_audio_identifiers_all.json', 'r') as f:
    identifiers = json.load(f)

# List to hold all metadata
all_metadata = []

# Loop through the identifiers and fetch metadata
for identifier in identifiers:
    print(f"Fetching metadata for {identifier}...")
    metadata_url = base_url + identifier  # Construct the metadata URL for each identifier
    
    try:
        response = requests.get(metadata_url)
        response.raise_for_status()  # Raise an exception if the request was unsuccessful
        metadata = response.json()  # Parse the JSON response
        
        # Append the metadata to the list
        all_metadata.append(metadata)
        
    except requests.exceptions.RequestException as e:
        print(f"Error fetching metadata for {identifier}: {e}")
        continue  # Skip this identifier and move to the next one

# Save the collected metadata to a file
with open('giant_bomb_audio_metadata.json', 'w') as f:
    json.dump(all_metadata, f, indent=4)

print("Metadata collection complete!")
