#!/usr/bin/env bash
curl http://localhost:5984/
curl -X PUT localhost:5984/_config/admins/admin -d '"admin"'
