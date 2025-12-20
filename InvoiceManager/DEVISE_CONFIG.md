# Configuration de la devise - Ariary Malgache (MGA)

## ?? Configuration actuelle

L'application InvoiceManager utilise maintenant l'**Ariary Malgache (Ar)** comme devise par défaut.

## ?? Modifications apportées

### 1. **Program.cs**
Configuration de la culture malgache :
```csharp
var cultureInfo = new CultureInfo("mg-MG");
cultureInfo.NumberFormat.CurrencySymbol = "Ar";
cultureInfo.NumberFormat.CurrencyDecimalDigits = 0; // Pas de centimes
cultureInfo.NumberFormat.CurrencyPositivePattern = 3; // Format: 25000 Ar
```

### 2. **CurrencyHelper.cs**
Helper pour formater les montants de manière cohérente :
```csharp
// Utilisation dans les composants Razor
@montant.ToString("C") // Affiche automatiquement en Ariary
@CurrencyHelper.FormatAriary(montant) // Format personnalisé
```

## ?? Format d'affichage

| Montant | Affichage |
|---------|-----------|
| 25000 | 25 000 Ar |
| 1500000 | 1 500 000 Ar |
| 750 | 750 Ar |

## ?? Caractéristiques de l'Ariary

- **Code ISO**: MGA
- **Symbole**: Ar
- **Subdivision**: 1 Ariary = 5 Iraimbilanja (non utilisé)
- **Pas de centimes** dans l'affichage
- **Séparateur de milliers**: espace

## ?? Pour revenir à l'Euro

Si vous souhaitez revenir à l'Euro, modifiez `Program.cs` :

```csharp
var cultureInfo = new CultureInfo("fr-FR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
```

## ?? Autres devises supportées

Pour changer vers une autre devise, utilisez les codes culture appropriés :

| Devise | Code Culture | Symbole |
|--------|-------------|---------|
| Euro | fr-FR | € |
| Dollar US | en-US | $ |
| Franc CFA | fr-CI | CFA |
| Ariary | mg-MG | Ar |

## ?? Notes importantes

1. Tous les montants dans l'application sont automatiquement formatés en Ariary
2. La base de données stocke toujours les valeurs en `decimal` (indépendant de la devise)
3. Le formatage est appliqué uniquement à l'affichage (`.ToString("C")`)
4. Les calculs restent précis même sans décimales à l'affichage

## ?? Utilisation dans les composants

### Affichage simple
```razor
<p>Total: @facture.TotalTTC.ToString("C")</p>
<!-- Affiche: Total: 25 000 Ar -->
```

### Avec le helper (optionnel)
```razor
<p>Total: @facture.TotalTTC.FormatAriary()</p>
<!-- Affiche: Total: 25 000 Ar -->
```

### Dans le code C#
```csharp
var montantFormate = CurrencyHelper.FormatCurrency(25000m);
// Résultat: "25 000 Ar"
```
