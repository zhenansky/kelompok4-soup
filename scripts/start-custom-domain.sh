#!/bin/bash
echo "Ì∫Ä Starting with custom domains..."
docker-compose -f docker-compose.custom-domain.yml down
docker-compose -f docker-compose.custom-domain.yml up -d --build
sleep 10
docker-compose -f docker-compose.custom-domain.yml ps
echo ""
echo "Ìºê Access your applications:"
echo "   Frontend: http://frontend.com"
echo "   API:     http://api.frontend.com:5000"
echo "   Admin:   http://admin.frontend.com:7000"
