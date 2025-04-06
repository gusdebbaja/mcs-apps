// PingResponse.java
package com.example.kapp.model;

public class PingResponse {
    private String source;
    private String message;

    public PingResponse() {
    }

    public PingResponse(String source, String message) {
        this.source = source;
        this.message = message;
    }

    public String getSource() {
        return source;
    }

    public void setSource(String source) {
        this.source = source;
    }

    public String getMessage() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }
}

