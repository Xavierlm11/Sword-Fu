# Sword Fu

Github: https://github.com/Xavierlm11/Sword-Fu

## 05/01/2024 (Patch):

Un pequeño error que hacía que los jugadores no pudieran atacar ha sido corregido.

## 31/12/2023:

### Novedades
En esta versión hemos rehecho el sistema para prescindir del antiguo sistema en el que había que introducir dos puertos distintos.
En este nuevo lanzamiento hemos implementado un sistema para enviar y recibir datos entre sesiones de forma sencilla, lo que nos
ha permitido replicar acciones en distintas sesiones y sincronizar estas.

### Funcionamiento
Para jugar, primero un jugador tiene que asumir el rol de host. Él debe seleccionar el botón de "Host", escribir su nombre y
crear una sala. Luego, los clientes deben darle al botón de "Client" y unirse al host poniendo su IP y un nombre. El nombre no
puede repetirse con otro jugador de la sala y el máximo de jugadores totales por sala son 4. Una vez todos los jugadores están dentro
el host puede empezar la partida. Los jugadores deben ganar la ronda eliminando a los demás, entonces ganará un punto y empezará una nueva
ronda. Aquel jugador que consiga ganar 3 rondas ganará la partida. Para jugar, usa __W/A/S/D__ para moverte, __click izquierdo__ para lanzar tu espada
y __click derecho__ para pararte y atacar cuerpo a cuerpo con tu otra espada. Ten cuidado, si lanzas tu espada, no podrás volver a hacerlo
hasta que la recogas del suelo. Además, ten en cuenta que tus ataques se lanzan en la dirección del personaje, no en el ratón.
Con __tabulador__ se muestran las puntuaciones de la partida y con __Escape__ un jugador puede pausar la partida y reanudarla de nuevo (ha de ser
el mismo quien haga ambas cosas). Por último, y a modo de debug, el host puede usar __F1__ para que todos los jugadores avancen a un nuevo
escenario, sobretodo en caso de que debido a las pérdidas de paquetes los jugadores queden desincronizados.

### Estructura
El código se basa en varios Singleton, siendo estos algunos de los más relevantes:

-NetworkManager: 
Tiene información y ajustes que se usan en el juego y en las conexiones, como el intervalo de tiempo en el que se envían packages 
para actualizar la posición de los jugadores. Además es un ScriptableObject en la carpeta Resources.
En el script también se ecuentran clases personalizadas para transeferir en packages, las cuales derivan de GenericSendClass y son
fácilmente serializables y deserialiables a Json.

-ConnectionManager:
Gestiona las conexiones de los jugadores y la replicación de paquetes, los cuales son enviados o recibidos en 2 threats respectivos.
El threat de enviar serializa todas las clases personalizadas a enviar que hay en una lista y las manda a los demás jugadores.
Hay diferentes configuraciones que se le pueden aplicar en la variable "transferType" de cada Custom Class. Dependiendo esta,
el paquete se enviará al host y, después, este lo reenviará a los destinatarios adecuados.

-GameManager:
Gestiona eventos generales de los clientes de la party en la party.

-GameplayManager:
Gestiona eventos del Game Loop, como los cambios de escena y puntuaciones.

### Problemas
Aunque un game loop correcto es posible, desde la conexión inicial de todos los jugadores hasta la victoria de uno, a menudo es
interrumpido por pérdidas de paquetes que desincronizan el estado de la partida, sobretodo cuando un jugador gana la ronda.

Además, aun tenemos que investigar los motivos, pero haciendo testing nos hemos dado cuenta que las desincronizaciones ocurren
con más frecuencia si es el Host quien gana la ronda.

### Actualizaciones futuras
-Sincronizar las animaciones de los personajes
-Implementar personajes diferentes para cada jugador
-Sincronizar mejor los ataques
-Añadir más información en la interfaz
-Mejorar los escenarios
-Implementar predicción en las acciones de los personajes
-Solventar problemas ocasionados por la pérdida de packetes o un orden incorrecto de su envío

### External

-Newtonsoft Json: 
https://github.com/JamesNK/Newtonsoft.Json
Hemos usado este package para poder serializar objectos dentro de otros objetos y ponerlos en un Json.
Para instalar en Unity: Window -> Package Manager -> Add package from git URL -> "com.unity.nuget.newtonsoft-json"

-UnityMainThreadDispatcher: 
https://github.com/PimDeWitte/UnityMainThreadDispatcher
Hemos usado esta clase para llamar a funciones en el threat principal

-Character models and animations:
https://www.mixamo.com/#/

-Scene assets:
https://assetstore.unity.com/packages/3d/environments/dungeons/lite-dungeon-pack-low-poly-3d-art-by-gridness-242692

-Visual Scripting:
Para instalar en Unity: Window -> Package Manager -> Add package from git URL -> "com.unity.visualscripting"

### Main Tasks

-Xavier Casadó: Package Replication System, players data and connections, in-game players synchronization

-Xavier López: Game Loop

-Albert Martín: Gameplay, Escenarios

----------------------------------------------------------------------------------------

## 21/11/2023:

### Novedades
En esta versión hemos prescindido del modo de conexión TCP para centrarnos solamente en UDP.
Hemos reestructrado todo el código para que ambos clientes sean capaces tanto de recibir información como de enviarla.
Además, también hemos rehecho el Lobby y conectado con una base de Gameplay.

### Funcionamiento
Inicia dos instancias de la aplicación. Una será el host y otra un cliente, uniéndose en ese orden. El host le dará al
botón de Create Game y podrá ponerse un nickname. También puede configurar el puerto que usará (Server Port) o el que deben
usar los clientes que se conecten a él (Client Port). Ambos campos tienen un botón de Default para establecerlos en 
5000 y 6000 respectivamente, lo cual viene por defecto y en la mayoría de ocasiones no es necesario cambiar. El botón de 
"Get Auto" obtendrá un puerto libre automáticamente, aunque ese puerto podría ocuparse al poco rato por algún proceso externo,
 por lo que habría que generar uno nuevo. Cuando estés listo, el botón "Create Room" te llevará a la sala de espera.
Por el lado del cliente, los campos de Nickname y puertos son iguales, pero hay un campo IP en el que hay que introducir la 
IP del servidor, y al finalizar hay que pulsar el botón "Join", el cual te llevará a la sala de espera con el servidor.

### Estructura
El código se basa principalmente en varios Singleton. El primero (que también es un ScriptableObject) se llama NetworkManager
y contiene datos de la configuración de la aplicación y clases básicas como la de cliente y otras usadas para el traspaso
de información entre clientes, además de datos sobre el cliente local. El LobbyManager maneja los menús y lo que ocurre cuando
 el usuario interactúa con botones o InputFields. El ConnectionManager gestiona los sockets que usan los clientes y las 
conexiones entre estos. El PartyManager gestion la lista de jugadores que hay en la sala y el PlayerManager controla la generación
de jugadores y la actualización de sus posiciones.


### Problemas
Teníamos un error que no supimos exactamente como solventar, ya que este se producía al llamar a la función Socket.Bind()
 por segunda vez con la misma dirección IP y puerto, cosa que hacíamos a la hora de crear varias instancias de la aplicación
en un mismo ordenador e intentar conectar los clientes, los cuales tenían la misma IP y puerto. Un Fix provisional ha sido
hacer que la aplicación use dos puertos. El host usará un puerto y el cliente otro, los cuales pueden ser modificados en la
aplicación. Aún así, la aplicación solo ha podido ser testeada con dos únicas instancias y en un mismo ordenador, y es 
importante que el Host se conecte primero y el otro cliente segundo.


### Main Tasks
-Xavi C: Reestructuración de todo el código y las conexiones entre clientes, reformas en los menús.
-Xavi L: Reestructuración de los menús y conexiones entre estos y el Gameplay.
-Albert M: Gameplay

### External

-Newtonsoft Json: 
https://github.com/JamesNK/Newtonsoft.Json
Hemos usado este package para poder serializar objectos dentro de otros objetos y ponerlos en un Json.

-UnityMainThreadDispatcher: 
https://github.com/PimDeWitte/UnityMainThreadDispatcher
Hemos usado esta clase para llamar a funciones en el threat principal
