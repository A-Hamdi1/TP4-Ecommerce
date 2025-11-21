# Ecommerce ASP.NET Core

Application e-commerce complète bâtie sur ASP.NET Core 8 (MVC) avec Entity Framework Core et Identity. Elle couvre la gestion de catalogue, les paniers, les commandes, les paiements Stripe/PayPal, l'envoi d'e-mails, la génération de factures PDF et un tableau de bord analytique pour les équipes métier.

## Fonctionnalités
- **Catalogue & catégories** : CRUD produit/catégorie, pagination, compteur de vues, filtrage par catégorie.
- **Expérience client** : panier persistant en session, wishlist, formulaire de contact, suivi de commande.
- **Checkout & paiements** : intégrations Stripe (PaymentIntent + webhook) et PayPal (Checkout Orders API), gestion du stock et de l’historique des statuts.
- **Services métier** : génération de factures PDF (QuestPDF), e-mails transactionnels (SMTP), analyse des ventes (Dashboard + API JSON).
- **Administration** : rôles Identity (`Admin`, `Manager`, etc.), gestion des utilisateurs/ordres, interface de pilotage (`DashboardController`).

## Architecture
- **ASP.NET Core MVC** (`Controllers`, `Views`, `ViewModels`) avec sessions pour le panier (`PanierController`, `ListeCart`).
- **Domaine & persistance** : `Models` (Product, Order, Commande, WishlistItem…), `Repositories` pour isoler l’accès EF Core (`AppDbContext`, migrations `Migrations/*`).
- **Services applicatifs** : `StripeService`, `PayPalService`, `EmailService`, `InvoiceService`, `AnalyticsService`.
- **Front-end** : Razor + Bootstrap/JS statiques dans `wwwroot` (css, js, lib).

## Technologies principales
- .NET 8 / ASP.NET Core MVC
- Entity Framework Core 9 + SQL Server (LocalDB par défaut)
- ASP.NET Core Identity
- Stripe.net 39, PayPal REST (HttpClient)
- QuestPDF 2025.7 pour les factures

## Prérequis
- .NET SDK 8.0.x
- SQL Server (LocalDB, Express ou Azure SQL)
- Accès SMTP (pour `EmailService`)
- Comptes Stripe et PayPal avec clés API

## Configuration
1. **Clonage & restauration**
   ```bash
   git clone <repo_url>
   cd Ecommerce
   dotnet restore
   ```
2. **Secrets & variables**  
   Déplacez les valeurs sensibles (`ConnectionStrings:ProductDBConnection`, section `Email`, `Stripe`, `PayPal`) hors de `appsettings.json` :
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:ProductDBConnection" "Server=.;Database=EcommerceDb;Trusted_Connection=True;"
   dotnet user-secrets set "Stripe:SecretKey" "<sk_live>"
   # ... idem pour PublishableKey, WebhookSecret, PayPal:ClientId, etc.
   ```
3. **Base de données**
   ```bash
   dotnet tool restore # si dotnet-ef listé dans manifest
   dotnet ef database update
   ```
4. **Comptes & rôles**  
   Lancez l’app, créez un compte via `/Account/Register`, puis attribuez-lui un rôle via l’interface admin (`/Admin/ListRoles`). Les contrôleurs protégés utilisent `[Authorize(Roles = "...")]`.

## Exécution
```bash
dotnet run
# ou
dotnet watch run
```
Site disponible sur `https://localhost:7141` (profil HTTPS de `launchSettings.json`).

## Structure (extrait)
```
Controllers/           # MVC controllers (Account, Admin, Product, Orders…)
Models/                # Entités EF + dépôts (Product, Order, Panier…)
Services/              # Email, Stripe, PayPal, Invoice, Analytics
ViewModels/            # Objets pour les vues Razor
Views/                 # Razor views (Home, Dashboard, Panier…)
wwwroot/               # Assets statiques (css, js, images, libs)
Migrations/            # Historique EF Core jusqu'à 2025-11-10
Program.cs             # Composition des services et pipeline HTTP
WebApplication2.csproj # Références NuGet & configuration cible net8.0
```

## Flux métier clés
- **Commande Stripe** : `OrdersController` crée la commande → `StripeService.CreatePaymentIntentAsync` → webhook Stripe déclenche `OrderRepository.UpdateStatus`.
- **Commande PayPal** : panier en session → `PayPalController.CreatePayment` rend la vue SDK → `CapturePayment` consomme l’API REST, confirme la commande et décrémente les stocks.
- **Analytics** : `DashboardController` consomme `IAnalyticsService` pour consolider ventes/journalières, revenus mensuels et classement produits.
- **Notifications** : `EmailService` envoie confirmations, statuts et PDF (générés par `InvoiceService`).

## Tests & qualité
Tests automatisés non fournis. Pour ajouter des tests :
- Créez un projet `xUnit`/`NUnit`.
- Mettez en place des doubles (InMemoryDb, test doubles d’`IEmailService`, etc.) pour tester repositories/services.

## Déploiement
```bash
dotnet publish -c Release -o publish
```
Le dossier `publish/` (déjà présent) contient un exemple de build autonome déployable sur IIS, Azure App Service ou conteneur. Pensez à configurer les variables d’environnement (connection string, SMTP, Stripe, PayPal) sur l’environnement cible.

## Sécurité & bonnes pratiques
- Remplacez les secrets d’exemple committés dans `appsettings.json`.
- Activez HTTPS (déjà configuré) et, en production, HSTS (`Program.cs`).
- Configurez les webhooks Stripe dans le portail Stripe en pointant vers `/Stripe/Webhook`.
- Surveillez les rôles Identity : seuls `Admin/Manager` accèdent aux contrôleurs critiques (`Dashboard`, `Admin`, `ManagerOrder`).

---
Pour toute contribution : ouvrez une issue décrivant la fonctionnalité/bug, proposez une PR avec description, captures d’écran si la vue change, et mettez à jour ce README si nécessaire.

