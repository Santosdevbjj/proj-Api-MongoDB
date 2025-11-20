## Construindo um Projeto de uma API.NET Integrada ao MongoDB.


<img width="1080" height="599" alt="Screenshot_20251014-030243" src="https://github.com/user-attachments/assets/3b99143c-8936-4315-b349-872aaadce727" />



---


**DESCRIÇÃO:**
Construiremos um projeto de uma API em .NET Core integrada a um cluster MongoDB, que também criaremos em tempo real no service cloud Mongo Atlas. Vamos repassar brevemente os conceitos básicos de front-end, back-end, bases de dados, NoSQL e MongoDB para fixar o entendimento e sua aplicabilidade.

---


# projApiMongoDB

API RESTful em **.NET 8** integrada ao **MongoDB** — template profissional e didático com:

- CRUD para a entidade `Infectado`
- GeoJSON + índice `2dsphere` para consultas geoespaciais eficientes (`/api/infectado/nearby`)
- Documentação via Swagger
- Exemplo de `Dockerfile` e `docker-compose` (com MongoDB)
- Testes unitários e de integração (Testcontainers)
- CI/CD (GitHub Actions) para build, testes e publish de imagem Docker
- Postman Collection pronta para import

---

## Estrutura do repositório

<img width="1080" height="1741" alt="Screenshot_20251014-025138" src="https://github.com/user-attachments/assets/eef0781a-1444-458b-bc79-f06fa379fa0c" />


---


## Tecnologias

- .NET 8 (C#)
- MongoDB (Atlas ou local/Docker)
- MongoDB.Driver (C#)
- Testcontainers for .NET (integração)
- xUnit / Moq
- Docker / Docker Compose
- GitHub Actions
- Postman

---

## Principais conceitos e decisões de design

### GeoJSON e índice 2dsphere
Armazenamos a posição geográfica em formato **GeoJSON** (`{ type: "Point", coordinates: [lon, lat] }`) no campo `location` do documento `Infectado`. Este formato permite criar índice `2dsphere` e executar consultas `$geoNear` muito eficientes.

### Repository Pattern
Separamos a camada de persistência (`Repositories`) da camada de API (`Controllers`) para facilitar testes, manutenção e evolução.

### DTOs e validação
Usamos DTOs para entrada de dados (evita over-posting) e validação de modelo (DataAnnotations / FluentValidation opcional).

### Testes com Testcontainers
Os testes de integração usam containers temporários do MongoDB para garantir ambiente limpo e previsível.

### CI/CD
Workflow de GitHub Actions que:
- compila e executa testes
- constrói e publica imagem Docker (configurada para Docker Hub; adaptável para GHCR)

---

## Como rodar localmente (passo a passo)

### 1) Com Docker Compose (recomendado)
```bash
git clone https://github.com/Santosdevbjj/projApiMongoDB.git
cd projApiMongoDB
docker-compose up --build


---
```


**API:** http://localhost:5000

**Swagger:** http://localhost:5000/swagger


**2) Local sem Docker (usando MongoDB local ou Atlas)**

Configure appsettings.json (ou variáveis de ambiente):

MongoDbSettings__ConnectionString

MongoDbSettings__DatabaseName

MongoDbSettings__InfectadosCollectionName


**Execute:**


cd src/projApiMongoDB.Api
dotnet run

**3) Testes**

Testes unitários e de integração:


dotnet test ./src/projApiMongoDB.Tests

> Para testes de integração que usam containers, assegure-se que Docker esteja rodando.




---

**Endpoints (resumo)**

GET /api/infectado — lista (page, pageSize)

GET /api/infectado/{id} — busca por id

POST /api/infectado — cria (body: dto com latitude/longitude)

PUT /api/infectado/{id} — atualiza

DELETE /api/infectado/{id} — remove

GET /api/infectado/nearby?lat=&lon=&maxKm=&limit= — busca geoespacial com $geoNear


**Exemplo POST body:**

{
  "dataNascimento": "1990-03-01T00:00:00Z",
  "sexo": "M",
  "latitude": -23.5630994,
  "longitude": -46.6565712
}


---

**Docker image / GitHub Actions**

Arquivo workflow .github/workflows/ci-cd.yml constrói, testa e publica imagem.

Configure secrets no GitHub: DOCKERHUB_USERNAME e DOCKERHUB_TOKEN (ou adapte para GHCR).



---

**Observações de segurança e produção**

Nunca versionar connection strings com credenciais.

Use variáveis de ambiente, Vault ou Secret Manager.

**Para produção:**

Monitore índices e performance do MongoDB.

Configure réplicas / backups.

Adicione autenticação/authorization (JWT, OAuth2).

Habilite TLS no MongoDB (Atlas já cuida disso).

Configure CORS, rate-limiting e logs estruturados.




---

**Próximos passos sugeridos (roadmap)**

Migrar validação para FluentValidation.

Adicionar autenticação (JWT).

Implementar DTOs de saída (ViewModels) com hiperlinks HATEOAS (opcional).

Centralizar erros (middleware) e retornar RFC7807 Problem Details.

Adicionar métricas (Prometheus) e tracing (OpenTelemetry).

Criar playground Postman/Collection Runner com exemplos e testes.



---

**Recursos / Links úteis**

MongoDB C# Driver: https://www.mongodb.com/docs/drivers/csharp/

MongoDB Geo Queries: https://www.mongodb.com/docs/manual/geospatial-queries/

Testcontainers for .NET: https://dotnet.testcontainers.org/

.NET docs: https://learn.microsoft.com/dotnet/



---

**Contribuição / Como subir para o GitHub**

1. git add .


2. git commit -m "feat(api): initial .NET + MongoDB project with geo and CI"


3. git push origin main



Se já houver repositório remoto, faça git pull antes.


---

**Contato**

[![Portfólio Sérgio Santos](https://img.shields.io/badge/Portfólio-Sérgio_Santos-111827?style=for-the-badge&logo=githubpages&logoColor=00eaff)](https://santosdevbjj.github.io/portfolio/)
[![LinkedIn Sérgio Santos](https://img.shields.io/badge/LinkedIn-Sérgio_Santos-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://linkedin.com/in/santossergioluiz) 



---



