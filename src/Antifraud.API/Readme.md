Desarrollado por Alejandro Maceda.

## Arquitectura de la Solución
Arquitectura en Capas (Layered Architecture). promoviendo la Separación de Preocupaciones (SoC).

### Herramientas y paquetes usados
- Net 8
- Dapper
- Postgresql 
- Kafka
- Fluentvalidation
- xUnit
- Moq

## Patrones de Diseño
### Repositorio (Repository Pattern):
Abstracción genérica con IDapperRepository<T> que define operaciones básicas de Dapper (ExecuteAsync, QueryAsync, QueryFirstOrDefaultAsync).
### Objeto de Transferencia de Datos (DTO):
La capa Antifraud.Dto se usa para transferir datos. Esta capa puede ser consumida por service.
### Capa de Servicio (Service Layer):
 Antifraud.Service es el centro de lógica de negocio, mediando entre Presentation y Repository.
### Modelo de Dominio (Domain Model) / Modelo Anémico:
El proyecto Antifraud.Model contiene clases que son representaciones de las tablas para Dapper.
### Service (Servicio de Trabajador en Segundo Plano):
Transaction.Worker y Antifraud.Worker dentro de Presentation para consumir mensajes de Kafka.
### Presentation (API)
Transaction.Api actúa como el principal punto de entrada para las interacciones externas con el sistema. Mediante una llamada al endpoint /api/transactions se hace la ejecución de todo el proceso.
## Ejemplos
```
curl -X 'POST' \
  'http://localhost:8080/api/Transactions' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "sourceAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetAccountId": "3fa85f64-5717-4550-b3fc-2c963f66afa6",
  "transferType": 1,
  "value": 1500.52
}'
```

- **transaction-api**: Expone una API REST (puerto 8080) para recibir solicitudes externas. Publica y consume mensajes de Kafka.
- **kafka**: Sistema de mensajería que permite la comunicación asíncrona entre los servicios. Depende de Zookeeper para la gestión del clúster.
- **zookeeper**: Servicio de coordinación necesario para Kafka.
- **transaction-worker**: Servicio de procesamiento que consume mensajes de Kafka y realiza operaciones relacionadas con transacciones.
- **antifraud-worker**: Servicio que procesa mensajes relacionados con la detección de fraude, también conectado a Kafka.
- **postgres**: Base de datos relacional donde se almacenan los datos de las transacciones y posiblemente los resultados del antifraude.
- **DB Volumen**: Permite la persistencia de los datos de Postgres.

## Flujo principal

1.	**transaction-api** recibe solicitudes y publica eventos en Kafka.
2.	**transaction-worker** y **antifraud-worker** consumen y publican eventos de Kafka (transactions y transactions-confirmed,respectivamente)procesan la lógica de negocio y actualizan o consultan la base de datos Postgres.
3.	Todos los servicios que requieren almacenamiento persistente interactúan con Postgres.
4.	Kafka utiliza Zookeeper para la gestión interna del clúster y la coordinación de los brokers.

### Diagrama mermaid
sequenceDiagram
    actor Usuario
    participant API as Transaction API
    participant Postgres
    participant Kafka
    participant Antifraud as Antifraud.Worker
    participant Worker as Transactions.Worker

    Usuario->>API: POST /api/transactions\n(sourceAccount, targetAccount, transferType, value)
    API->>API: Validaciones (no nulos, valores válidos)
    API->>Postgres: Insertar transaction y transaction-event
    Postgres->>API: Confirma inserción.
    API->>Kafka: Publicar en topic "transactions"

    Kafka->>Antifraud: Consumir "transactions"
    Antifraud->>Antifraud: Validaciones:\n- Existe y no procesada\n- Valor <= 2000\n- Acumulado diario <= 20000
    Antifraud->>Kafka: Publicar en topic "transactions-confirmed"

    Kafka->>Worker: Consumir "transactions-confirmed"
    Worker->>Postgres: Actualizar estado de transaction-event y transaction
    Postgres->>Worker: Notifica actualización exitosa
    worker->>Usuario: Notifica resultado transacción (future)

## Quick Start
Para iniciar la solución por favor clona el proyecto, cambia la rama a dev.
Una vez que se tenga el proyecto, ir a la carpeta ``antifraud.net\src\Antifraud.API``. Abrir la ruta en un powershell.

Ejecutar el siguiente comando docker:
``docker compose up -d``

Todos los servicios deben de levantar.

## Tools and tips
Existe la carpeta ``docs\scripts``, en donde hay dos archivos:
- create-topics.sh: sirve para registrar los topics de kafka ``transactions`` y ``transactions-confirmed``. Este script es ejecutado en un contenedor docker registrado en el ``docker-compose``
- init-db.sql: contiene queries para la creación del esquema y tablas necesarias para la aplicación