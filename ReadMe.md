# Projet Let's Go Biking - Middleware

## Comment démarrer le projet ?

Pour lancer le serveur et le proxy/cache, il faut executer en mode administrateur le fichier *"Launch.bat"*

Pour l'instant le client ne veut pas se lancer en executant le .jar, il faut alors l'ouvrir dans un projet Intellij (ou autres IDE).

## Comment fonctionne le projet ?

### Coté client java :
Dans le terminal du client, est demandé à l'utilisateur d'entrer l'adresse de départ, puis l'adresse d'arrivée.
Ces deux adresses sont envoyées au serveur pour traitement. 

***! Voir traitement coté serveur !***

Le client récupère du serveur un tuple contenant l'ID de la queue ActiveMQ, la liste des étapes et la liste des coordonnées des étapes.

Il lance la carte avec le tuple recuperé.

Il lance la méthode *"messageFromActiveMQ()"* qui permet de récupérer les étapes dans la queue créer par le serveur. Si le client n'arrive pas à se connecter à *ActiveMQ*, alors la méthode lance une autre méthode *withoutActiveMQ()* qui permet d'afficher les étapes sans *ActiveMQ*.

Cette méthode *"withoutActiveMQ()"* peut être directement si le client ne veut pas utiliser *ActiveMQ*.

La carte s'affiche avec le point de départ, le point d'arrivée et le trajet.

### Coté serveur :

Un fois les adresses récuperées par le serveur, il utilise la méthode *"GetGeometry()"* qui permet de récupérer les coordonnées des adresses grâce à l'API *OpenCageData*

Avec les coordonnées des adresses, on détermine le contrat le plus proche. On utilise la méthode *"getBestContract()"*, qui va boucler sur la liste des contrats disponibles et recuperer le contrat le plus proche de nos coordonnées (départ et arrivée) en utilisant de nouveau l'API *OpenCageData*.

On récupère la liste des stations qui correspondent aux contrats des points d'arrivée et de départ. Pour ce faire on fait une requête au proxy grâce à la méthode *getStationFromAContractAsync()*

***! Voir traitement coté proxy/cache !***

On veut maintenant récupérer la station la plus proche de notre point de départ, la station la plus proche de notre point d'arrivée et le trajet entre le départ vers la premiere station, la première station vers la deuxième station, la deuxième station vers le point d'arrivée. On utilise la méthode *getGeojson()*. Cette méthode en plus de récupérer les stations les plus proches de nos points, elle va aussi récupérer les informations de trajet avec l'API *OpenRouteService* qui est dans la méthode *geoJsonRequest()*. 

On calcule maintenant le temps pour savoir si le trajet est plus rapide en vélo ou à pied. En fonction de ca on ajoute les étapes dans une liste et les coordonnées de chaque étape dans une autre liste.

Ces deux listes sont envoyées dans une méthode *activeMQ()* qui permet d'initialiser une connexion à *ActiveMQ* et de renvoyer au client l'ID de la queue ainsi que la liste des étapes et les coordonnées des étapes si le client ne veut pas / ou ne peux pas / utiliser ActiveMQ.

***! Voir suite du traitement coté client !***

### Coté proxy/cache :

Le proxy regarde dans son cache si le contrat demandé a déjà été demander et enregistrer. 

Si c'est le cas il renvoie ce qu'il a dans son cache.
Sinon il fait une requête à l'API JCDecaux avec le contrat demandé.

Remarque :

Le proxy interroge son cache seulement pour demander la liste des stations d'un contrat spécifique.

Pour récupérer la liste des contrats, il fait une requête directement à JCDecaux.

Le proxy desérialise directement le Json pour envoyer un objet au serveur.

***! Voir suite du traitement coté serveur !***

#### ***Julien VASSARD - SI4 - Middleware - Polytech Nice Sophia***




