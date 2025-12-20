# 🚀 InvoiceManager - Déploiement GitHub Codespaces

## 📋 Prérequis

- Compte GitHub avec accès à Codespaces
- Connexion Internet stable

## 🛠️ Configuration initiale

### **1. Démarrer Codespaces**

```sh
# Dans GitHub, cliquez sur "Code" > "Codespaces" > "Create codespace"
```

### **2. Restaurer les dépendances**

```sh
cd InvoiceManager
dotnet restore
```

### **3. Vérifier l'encodage**

```powershell
# Sur Windows/PowerShell
.\check-encoding.ps1

# Sur Linux/Mac
chmod +x download-bootstrap-icons.sh
./download-bootstrap-icons.sh
```

### **4. Lancer l'application**

```sh
dotnet run
```

### **5. Accéder à l'application**

Codespaces ouvrira automatiquement un port. Cliquez sur "Open in Browser" ou utilisez l'URL fournie.

## 🐛 Problèmes courants

### **Problème 1: Icônes Bootstrap ne s'affichent pas**

**Cause**: CDN bloqué par le pare-feu Codespaces

**Solution**:
```sh
# Télécharger les icônes localement
chmod +x download-bootstrap-icons.sh
./download-bootstrap-icons.sh
```

Puis modifier `App.razor`:
```html
<!-- Remplacer -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />

<!-- Par -->
<link rel="stylesheet" href="/fonts/bootstrap-icons.min.css" />
```

### **Problème 2: Caractères accentués cassés (é → Ã©)**

**Cause**: Encodage UTF-8 non configuré

**Solution**: Les corrections sont déjà appliquées dans:
- `App.razor` - Balises meta UTF-8
- `Program.cs` - Encodage forcé
- `.editorconfig` - Configuration de l'éditeur

**Vérification**:
```sh
# Vérifier les headers HTTP
curl -I http://localhost:5000

# Devrait afficher: Content-Type: text/html; charset=utf-8
```

### **Problème 3: Port non accessible**

**Solution**:
1. Dans Codespaces, allez dans l'onglet "PORTS"
2. Cliquez sur le port 5000
3. Changez la visibilité en "Public"

### **Problème 4: Base de données SQLite bloquée**

**Solution**:
```sh
# Supprimer et recréer la base
rm InvoiceManager/invoice_manager.db*
dotnet run
```

## 🔍 Diagnostic avancé

### **Vérifier l'encodage des fichiers**

```sh
# Linux/Mac
file -i InvoiceManager/Components/Pages/*.razor

# PowerShell
.\check-encoding.ps1
```

### **Tester la connectivité CDN**

```sh
curl -I https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css
```

### **Vérifier les logs**

```sh
dotnet run --verbosity detailed 2>&1 | tee app.log
```

## 📦 Structure des fichiers

```
InvoiceManager/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor       # UTF-8-BOM ✓
│   │   ├── Clients.razor    # UTF-8-BOM ✓
│   │   └── Factures.razor   # UTF-8-BOM ✓
│   ├── App.razor            # Meta charset ✓
│   └── _Imports.razor       # UTF-8-BOM ✓
├── wwwroot/
│   └── fonts/               # Bootstrap Icons (local)
├── Program.cs               # Encodage UTF-8 forcé ✓
├── .editorconfig            # Configuration encodage ✓
└── web.config               # Headers UTF-8 ✓
```

## ✅ Checklist de déploiement

- [ ] Codespaces créé et démarré
- [ ] `dotnet restore` exécuté
- [ ] Encodage UTF-8 vérifié (`check-encoding.ps1`)
- [ ] Bootstrap Icons téléchargé localement (si CDN bloqué)
- [ ] Application lancée (`dotnet run`)
- [ ] Port 5000 accessible et public
- [ ] Icônes s'affichent correctement
- [ ] Caractères accentués (é, è, à) s'affichent correctement
- [ ] Montants en Ariary affichés avec points (25.000 Ar)

## 🎯 Tests fonctionnels

### **1. Page d'accueil**
- ✅ Tableau de bord avec statistiques
- ✅ Icônes Bootstrap visibles (personnes, fichiers, argent)
- ✅ Texte "Bienvenue dans votre gestionnaire de factures"

### **2. Page Clients**
- ✅ Bouton "Nouveau Client" avec icône +
- ✅ Liste des clients
- ✅ Caractères accentués dans "Téléphone", "Créé"

### **3. Page Factures**
- ✅ Bouton "Nouvelle Facture" avec icône +
- ✅ Badges de statut (Brouillon, Validée, Annulée)
- ✅ Montants en Ariary avec points: 25.000 Ar

## 🔧 Configuration Codespaces avancée

### **Créer `.devcontainer/devcontainer.json`**

```json
{
  "name": "InvoiceManager .NET 8",
  "image": "mcr.microsoft.com/devcontainers/dotnet:8.0",
  "forwardPorts": [5000, 5001],
  "postCreateCommand": "dotnet restore && dotnet build",
  "customizations": {
    "vscode": {
      "extensions": [
        "ms-dotnettools.csharp",
        "ms-dotnettools.csdevkit"
      ],
      "settings": {
        "files.encoding": "utf8bom",
        "files.autoSave": "afterDelay"
      }
    }
  },
  "features": {
    "ghcr.io/devcontainers/features/dotnet:2": {
      "version": "8.0"
    }
  }
}
```

## 📞 Support

En cas de problème persistant:

1. **Consultez** `TROUBLESHOOTING_ENCODAGE.md`
2. **Vérifiez** les logs: `dotnet run --verbosity detailed`
3. **Redémarrez** Codespaces: Code > Codespaces > Rebuild Container

## 🌐 Liens utiles

- [GitHub Codespaces Docs](https://docs.github.com/en/codespaces)
- [ASP.NET Core Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Bootstrap Icons](https://icons.getbootstrap.com/)

---

**Note**: Tous les problèmes d'encodage et d'icônes ont été résolus dans cette version. Si vous rencontrez encore des problèmes, c'est probablement lié à la configuration réseau de Codespaces.
