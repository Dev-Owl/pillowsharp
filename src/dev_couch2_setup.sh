#!/usr/bin/env bash
curl http://localhost:5984/
curl -H "Content-Type: application/json" -X POST -d '{"action":"enable_single_node","username":"admin","password":"admin","bind_address":"0.0.0.0","port":5984,"singlenode":true}' http://localhost:5984/_cluster_setup
