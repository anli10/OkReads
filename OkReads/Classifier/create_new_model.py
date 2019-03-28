import argparse
import pandas as pd

from classifier import Classifier

def main():
    parser = argparse.ArgumentParser(description="Populate sentiment fields in SQLite database")
    parser.add_argument("--filename", type=str, help="Input file for training")
    args = parser.parse_args()

    # Train the network
    df = pd.read_json(args.filename, orient="records")
    df = df[['description', 'genre']].dropna(how='any')
    (X, y) = (df['description'], df['genre'])
    # TODO: Balance dataset by having classes with equal probability
    model = Classifier()
    model.fit(X, y, epochs=10)
    model.save_to_file("model.h5", "tokenizer", "binarizer")

if __name__ == "__main__":
    main()