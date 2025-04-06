from flask import Flask, render_template, jsonify, request
import requests

app = Flask(__name__)

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/ping-w', methods=['POST'])
def ping_w():
    try:
        response = requests.post('http://Windows-Omni-1/api/ping')
        return jsonify({"status": "success", "message": response.json()})
    except Exception as e:
        return jsonify({"status": "error", "message": str(e)})

@app.route('/ping-k', methods=['POST'])
def ping_k():
    try:
        response = requests.post('http://k8s-app-url/api/ping')
        return jsonify({"status": "success", "message": response.json()})
    except Exception as e:
        return jsonify({"status": "error", "message": str(e)})

@app.route('/cat-fact', methods=['GET'])
def cat_fact():
    try:
        response = requests.get('https://catfact.ninja/fact')
        return jsonify({"status": "success", "data": response.json()})
    except Exception as e:
        return jsonify({"status": "error", "message": str(e)})

@app.route('/api/ping', methods=['POST'])
def receive_ping():
    return jsonify({"source": "Python App (L)", "message": "Received ping!"})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)