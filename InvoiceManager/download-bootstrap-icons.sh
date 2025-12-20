#!/bin/bash
# Script pour télécharger Bootstrap Icons localement
# Usage: ./download-bootstrap-icons.sh

echo "=== Téléchargement de Bootstrap Icons ==="

# Créer le dossier
mkdir -p InvoiceManager/wwwroot/fonts

cd InvoiceManager/wwwroot/fonts

# Télécharger Bootstrap Icons
echo "Téléchargement depuis GitHub..."
wget -q https://github.com/twbs/icons/releases/download/v1.11.3/bootstrap-icons-1.11.3.zip -O bootstrap-icons.zip

if [ $? -eq 0 ]; then
    echo "✓ Téléchargement réussi"
    
    # Décompresser
    echo "Décompression..."
    unzip -q bootstrap-icons.zip
    
    # Copier les fichiers nécessaires
    echo "Copie des fichiers CSS et fonts..."
    cp bootstrap-icons-1.11.3/font/bootstrap-icons.min.css .
    cp -r bootstrap-icons-1.11.3/font/fonts .
    
    # Nettoyer
    echo "Nettoyage..."
    rm bootstrap-icons.zip
    rm -rf bootstrap-icons-1.11.3
    
    echo "✓ Bootstrap Icons installé localement!"
    echo ""
    echo "Mettez à jour App.razor:"
    echo '  <link rel="stylesheet" href="/fonts/bootstrap-icons.min.css" />'
else
    echo "✗ Erreur lors du téléchargement"
    echo "Essayez manuellement:"
    echo "  1. Allez sur https://github.com/twbs/icons/releases"
    echo "  2. Téléchargez bootstrap-icons-1.11.3.zip"
    echo "  3. Décompressez dans wwwroot/fonts/"
fi
