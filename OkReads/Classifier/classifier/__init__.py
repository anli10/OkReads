import pickle

import numpy as np
import pandas as pd
import tensorflow as tf
from sklearn.preprocessing import LabelBinarizer
from keras.preprocessing.text import Tokenizer
from keras.models import Sequential, load_model, save_model
from keras.layers import Dense, Activation

class Classifier:
    def __init__(self):
        self._isInitialized = False

    def _initializeModel(self, input_shape, output_shape):
        self._model = Sequential([Dense(128, input_shape=input_shape),
                                Activation('relu'),
                                Dense(output_shape),
                                Activation('softmax'),
                                ])
        self._model.compile(optimizer='adam',
                loss='categorical_crossentropy',
                metrics=['accuracy'])
        self._isInitialized = True

    def load_from_file(self, model_filename, tokenizer_filename, binarizer_filename):
        self._model = load_model(model_filename)
        with open(tokenizer_filename, "rb") as f:
            self._tokenizer = pickle.load(f)
        with open(binarizer_filename, "rb") as f:
            self._binarizer = pickle.load(f)
        self._isInitialized = True

    def save_to_file(self, model_filename, tokenizer_filename, binarizer_filename):
        save_model(self._model, model_filename)
        with open(tokenizer_filename, "wb") as f:
            pickle.dump(self._tokenizer, f)
        with open(binarizer_filename, "wb") as f:
            pickle.dump(self._binarizer, f)

    def fit(self, X, y, epochs=2, batch_size=32):
        if not self._isInitialized:
            self._tokenizer = Tokenizer()
            self._binarizer = LabelBinarizer()

        # Preprocess input data
        self._tokenizer.fit_on_texts(X)
        X = self._tokenizer.texts_to_matrix(X, mode='tfidf')

        # Preprocess labels
        y = self._binarizer.fit_transform(y)

        if not self._isInitialized:
            self._initializeModel((X.shape[1], ), y.shape[1])

        self._model.fit(X, y, epochs=epochs, batch_size=batch_size,
                        validation_split=0.1)

    def predict(self, X):
        X = self._tokenizer.texts_to_matrix(X)
        return self._binarizer.inverse_transform(self._model.predict(X))

def train_model(filename):
    # Load dataset
    df = pd.read_json(filename, orient="records")

    # Keep only description and genre columns
    df = df[['description', 'genre']].dropna(how='any')

    # Setup model
    X = df['description']
    y = df['genre']
    model = Classifier()
    model.fit(X, y)
    return model