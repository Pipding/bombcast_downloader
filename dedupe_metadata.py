import json

# Load the metadata from the original file
with open('giant_bomb_audio_metadata_original.json', 'r') as f:
    metadata_list = json.load(f)

# Dictionary to store unique entries, with the identifier as the key
unique_metadata = {}

# Iterate through the list and add unique entries based on the 'metadata.identifier'
for entry in metadata_list:
    identifier = entry['metadata']['identifier']  # Extract the identifier
    # Only add the entry if it's not already in the dictionary
    if identifier not in unique_metadata:
        unique_metadata[identifier] = entry

# Get the values (unique entries) from the dictionary
de_duplicated_metadata = list(unique_metadata.values())

# Save the de-duplicated list to a new JSON file
with open('giant_bomb_audio_metadata_deduplicated.json', 'w') as f:
    json.dump(de_duplicated_metadata, f, indent=4)

print(f"De-duplication complete. {len(de_duplicated_metadata)} unique entries saved.")
