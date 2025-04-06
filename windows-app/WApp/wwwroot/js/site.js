// This file can be used for any additional JavaScript you might need
function pingApp(target) {
    document.getElementById('response').innerHTML = 'Loading...';
    
    let endpoint = '';
    if (target === 'l') {
        endpoint = '/api/ping-l';
    } else if (target === 'k') {
        endpoint = '/api/ping-k';
    }
    
    fetch(endpoint, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => response.json())
    .then(data => {
        document.getElementById('response').innerHTML = 
            '<pre>' + JSON.stringify(data, null, 2) + '</pre>';
    })
    .catch(error => {
        document.getElementById('response').innerHTML = 
            '<p style="color: red;">Error: ' + error + '</p>';
    });
}

function getCatFact() {
    document.getElementById('response').innerHTML = 'Loading...';
    
    fetch('/api/cat-fact')
    .then(response => response.json())
    .then(data => {
        document.getElementById('response').innerHTML = 
            '<pre>' + JSON.stringify(data, null, 2) + '</pre>';
    })
    .catch(error => {
        document.getElementById('response').innerHTML = 
            '<p style="color: red;">Error: ' + error + '</p>';
    });
}