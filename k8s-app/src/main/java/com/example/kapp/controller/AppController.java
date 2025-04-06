package com.example.kapp.controller;

import com.example.kapp.model.PingResponse;
import com.example.kapp.model.Response;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.client.RestTemplate;

import java.util.HashMap;
import java.util.Map;

@Controller
public class AppController {

    @Autowired
    private RestTemplate restTemplate;

    @GetMapping("/")
    public String index(Model model) {
        return "index";
    }

    @PostMapping("/api/ping")
    @ResponseBody
    public PingResponse receivePing() {
        return new PingResponse("Java App (K)", "Received ping!");
    }

    @PostMapping("/api/ping-l")
    @ResponseBody
    public Response pingL() {
        try {
            ResponseEntity<PingResponse> response = restTemplate.postForEntity(
                "http://192.168.1.111/api/ping", null, PingResponse.class);
            
            return new Response("success", response.getBody());
        } catch (Exception e) {
            return new Response("error", e.getMessage());
        }
    }

    @PostMapping("/api/ping-w")
    @ResponseBody
    public Response pingW() {
        try {
            ResponseEntity<PingResponse> response = restTemplate.postForEntity(
                "http://Windows-Omni-1/api/ping", null, PingResponse.class);
            
            return new Response("success", response.getBody());
        } catch (Exception e) {
            return new Response("error", e.getMessage());
        }
    }

    @GetMapping("/api/cat-fact")
    @ResponseBody
    public Response getCatFact() {
        try {
            ResponseEntity<Map> response = restTemplate.getForEntity(
                "https://catfact.ninja/fact", Map.class);
            
            return new Response("success", response.getBody());
        } catch (Exception e) {
            return new Response("error", e.getMessage());
        }
    }
}