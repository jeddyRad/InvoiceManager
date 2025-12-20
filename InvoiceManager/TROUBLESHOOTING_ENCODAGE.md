# 🔧 Guide de diagnostic - Problèmes d'encodage et icônes

## 🐛 Symptômes

- ❌ Les icônes Bootstrap ne s'affichent pas
- ❌ Les caractères accentués (é, è, à, ç, etc.) sont cassés
- ❌ Affichage de � ou de caractères bizarres

## 📋 Causes possibles

### 1. **Encodage des fichiers**
Les fichiers ne sont pas sauvegardés en UTF-8

### 2. **Headers HTTP manquants**
Le serveur n'envoie pas les bons headers `Content-Type`

### 3. **CDN bloqué (GitHub Codespaces)**
Les CDN externes peuvent être bloqués par le pare-feu

### 4. **Proxy/Firewall**
GitHub Codespaces peut bloquer certaines ressources

## ✅ Solutions implémentées

### **1. App.razor**
```html
<meta charset="utf-8" />
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
```

### **2. Program.cs**
```csharp
// Forcer l'encodage UTF-8
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;

// Headers HTTP
app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Type"] = "text/html; charset=utf-8";
    await next();
});
```

### **3. .editorconfig**
Garantit que tous les fichiers sont sauvegardés en UTF-8

### **4. web.config**
Configuration pour IIS/Azure avec encodage UTF-8

## 🔍 Diagnostic dans GitHub Codespaces

### **Tester l'encodage**
```sh
# Vérifier l'encodage des fichiers
file -i InvoiceManager/Components/Pages/*.razor

# Devrait afficher: charset=utf-8
```

### **Vérifier les headers HTTP**
```sh
# Lancer l'application
dotnet run

# Dans un autre terminal
curl -I http://localhost:5000

# Vérifier: Content-Type: text/html; charset=utf-8
```

### **Tester les icônes Bootstrap**
```sh
# Vérifier si le CDN est accessible
curl -I https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css

# Devrait retourner: HTTP/2 200
```

## 🚀 Solutions alternatives pour GitHub Codespaces

### **Option 1: Copier les icônes localement**

Si le CDN est bloqué, téléchargez Bootstrap Icons localement :

```sh
cd InvoiceManager/wwwroot
mkdir fonts
cd fonts
wget https://github.com/twbs/icons/releases/download/v1.11.3/bootstrap-icons-1.11.3.zip
unzip bootstrap-icons-1.11.3.zip
```

Puis dans `App.razor` :
```html
<!-- Remplacer -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />

<!-- Par -->
<link rel="stylesheet" href="/fonts/bootstrap-icons.min.css" />
```

### **Option 2: Utiliser Font Awesome (alternative)**

```html
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
```

### **Option 3: Configurer le proxy GitHub Codespaces**

Créer `.devcontainer/devcontainer.json` :
```json
{
  "name": "InvoiceManager",
  "forwardPorts": [5000, 5001],
  "postCreateCommand": "dotnet restore",
  "customizations": {
    "vscode": {
      "extensions": [
        "ms-dotnettools.csharp"
      ]
    }
  }
}
```

## 🔬 Tests à effectuer

### **1. Test des caractères accentués**
Ouvrir l'application et vérifier :
- ✅ "Éditer" s'affiche correctement (pas "Ãditer")
- ✅ "Créé" s'affiche correctement
- ✅ "Numéro" s'affiche correctement
- ✅ "Validée" s'affiche correctement

### **2. Test des icônes Bootstrap**
Vérifier que ces icônes s'affichent :
- ✅ `<i class="bi bi-plus-circle"></i>` (Plus)
- ✅ `<i class="bi bi-people-fill"></i>` (Personnes)
- ✅ `<i class="bi bi-file-earmark-text-fill"></i>` (Document)
- ✅ `<i class="bi bi-cash-stack"></i>` (Argent)
- ✅ `<i class="bi bi-arrow-clockwise"></i>` (Actualiser)

### **3. Test du formatage Ariary**
Vérifier l'affichage :
- ✅ `25.000 Ar` (avec point, pas d'erreur d'encodage)

## 📊 Checklist de déploiement

- [ ] Tous les fichiers `.razor` sont en UTF-8-BOM
- [ ] Tous les fichiers `.cs` sont en UTF-8-BOM
- [ ] `App.razor` a les bonnes balises meta
- [ ] `Program.cs` force l'encodage UTF-8
- [ ] `.editorconfig` est présent
- [ ] Les icônes Bootstrap s'affichent localement
- [ ] Les caractères accentués s'affichent localement
- [ ] L'application fonctionne dans GitHub Codespaces
- [ ] Les headers HTTP sont corrects (curl -I)

## 🆘 Si le problème persiste

### **Vérifier les logs**
```sh
dotnet run --verbosity detailed
```

### **Forcer la recompilation**
```sh
dotnet clean
dotnet build
dotnet run
```

### **Vider le cache du navigateur**
Dans GitHub Codespaces, faire `Ctrl+Shift+R` (hard reload)

### **Vérifier le charset de la réponse**
Ouvrir les DevTools (F12) > Network > Cliquer sur le document HTML > Headers
Chercher : `Content-Type: text/html; charset=utf-8`

## 📝 Notes GitHub Codespaces

- GitHub Codespaces utilise des conteneurs Docker
- Certains CDN peuvent être bloqués par défaut
- Les ports doivent être correctement transférés (port forwarding)
- L'encodage UTF-8 doit être explicitement configuré

## 🔗 Liens utiles

- [Bootstrap Icons](https://icons.getbootstrap.com/)
- [ASP.NET Core Globalization](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization)
- [GitHub Codespaces Docs](https://docs.github.com/en/codespaces)
