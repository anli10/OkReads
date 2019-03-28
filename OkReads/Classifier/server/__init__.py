from http.server import HTTPServer, BaseHTTPRequestHandler
import urllib.parse as urlparse

from classifier import Classifier

class ClassifierServer(HTTPServer):
    clf = Classifier()

    def __init__(self, server_address, RequestHandlerClass):
        self.clf.load_from_file("model.h5", "tokenizer", "binarizer")
        super().__init__(server_address, RequestHandlerClass)

class ClassiefierRequestHandler(BaseHTTPRequestHandler):
    def __init__(self, request, client_address, server):
        super().__init__(request, client_address, server)

    def do_GET(self):
        parsed = urlparse.urlparse(self.path)
        try:
            description = urlparse.parse_qs(parsed.query)["description"]
        except KeyError as e:
            self.send_response(400)
            return
        self.send_response(200)
        self.send_header("Content-type", "text/json")
        self.end_headers()
        print(description)
        prediction = self.server.clf.predict([description])[0]
        message = '{ "Genre" : "' + prediction + '" }'
        self.wfile.write(bytes(message, "utf-8"))

def runClassifierService(server_address="", port=8000):
    server_address = ('', port)
    httpd = ClassifierServer(server_address, ClassiefierRequestHandler)
    print("[#] Running classifier service on {0}:{1}".format(server_address, port))
    httpd.serve_forever()