# API .NET 8 + MongoDB — Geolocalização de Casos com $geoNear

> **API RESTful com consultas geoespaciais reais:** CRUD de registros epidemiológicos com busca por proximidade usando GeoJSON + índice `2dsphere` do MongoDB, testes de integração com Testcontainers e pipeline CI/CD com publicação de imagem Docker no GHCR.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![MongoDB](https://img.shields.io/badge/MongoDB-6.0-47A248?style=for-the-badge&logo=mongodb&logoColor=white)](https://www.mongodb.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![Testcontainers](https://img.shields.io/badge/Testcontainers-.NET-000000?style=for-the-badge&logo=docker&logoColor=white)](https://dotnet.testcontainers.org/)
[![CI](https://img.shields.io/github/actions/workflow/status/Santosdevbjj/proj-Api-MongoDB/dotnet.yml?style=for-the-badge&logo=githubactions&logoColor=white)](https://github.com/Santosdevbjj/proj-Api-MongoDB/actions)
[![License](https://img.shields.io/badge/Licença-MIT-green?style=for-the-badge)](LICENSE)

---

## 1. Problema de Negócio

Sistemas de vigilância epidemiológica e rastreamento de casos dependem de uma capacidade fundamental que bancos de dados relacionais tradicionais entregam com dificuldade: **consultas baseadas em distância geográfica real**. Perguntas como "quais casos registrados estão a menos de 5 km da minha localização?" exigem cálculos de distância em superfície esférica — não coordenadas brutas — e retorno ordenado por proximidade.

Implementar isso com SQL requer extensões geoespaciais, funções trigonométricas e índices especializados que variam por banco de dados. O resultado é código frágil, difícil de manter e com performance degradada em volumes maiores.

O problema concreto que este projeto endereça é duplo:

- **Ausência de integração entre .NET e MongoDB com suporte geoespacial nativo:** a maioria dos tutoriais de .NET + MongoDB demonstra CRUD básico. Nenhum demonstra como usar `$geoNear` com o agregador do MongoDB.Driver C#, que tem uma API diferente do que a documentação oficial sugere.
- **Testes de integração com banco real sem infraestrutura fixa:** testar queries MongoDB reais — especialmente `$geoNear` — requer um banco MongoDB de verdade. Testcontainers resolve isso sem depender de instâncias compartilhadas ou mocks que não validam o comportamento real do banco.

**A questão central:** como construir uma API .NET que faça consultas geoespaciais reais no MongoDB, com índice `2dsphere` correto, testes de integração que validem o comportamento geoespacial e pipeline que publique a imagem automaticamente?

---

## 2. Contexto

O projeto usa o domínio de registros epidemiológicos como contexto para demonstrar capacidades técnicas que vão além do CRUD: armazenamento de posição geográfica em formato GeoJSON, criação de índice `2dsphere` no startup da aplicação e execução de pipeline de agregação `$geoNear` para busca por proximidade ordenada por distância crescente.

A escolha do MongoDB como banco de dados não foi arbitrária. Documentos com campos de geometria variável — como `location` com GeoJSON — se encaixam naturalmente no modelo de documento do MongoDB. O campo `location` com `{ type: "Point", coordinates: [lon, lat] }` é o formato que o MongoDB espera para o índice `2dsphere`, e o C# Driver tem suporte nativo a esse tipo via `GeoJsonPoint`.

Este projeto também serve como referência prática para conectar .NET 8 ao MongoDB Atlas em cloud, substituindo a `ConnectionString` local pela string do Atlas — algo comum em projetos corporativos que usam MongoDB como banco principal de microserviços de localização.

---

## 3. Premissas

Para estruturar este projeto, foram adotadas as seguintes premissas:

- O campo `location` segue o padrão GeoJSON com **longitude antes de latitude** no array de coordenadas (`[lon, lat]`), conforme a especificação GeoJSON RFC 7946. Inverter a ordem é o erro mais comum em implementações geoespaciais com MongoDB.
- O índice `2dsphere` é criado no construtor do `InfectadoRepository` via `EnsureIndexes()`. O método `Indexes.CreateOne()` é idempotente — chamar com o mesmo índice existente não lança exceção.
- Os testes de integração usam Testcontainers para subir um container MongoDB real a cada execução. Isso garante que as queries `$geoNear` são testadas contra o banco real, não contra mocks que não validam o comportamento geoespacial.
- Connection strings nunca são versionadas. O `appsettings.json` contém apenas a configuração de desenvolvimento local. Em produção, as variáveis de ambiente sobrescrevem os valores via `docker-compose` ou orchestrador.
- Paginação é implementada no `GetAllAsync` com `Skip` e `Limit` para evitar retorno irrestrito de documentos em coleções grandes.

---

## 4. Estratégia da Solução

**GeoJSON + índice `2dsphere` para consultas esféricas reais**

O modelo `Infectado` armazena a posição no campo `location` como um `GeoJsonPoint` customizado com `{ type: "Point", coordinates: [longitude, latitude] }`. O `InfectadoRepository` cria o índice `2dsphere` no campo `location` no startup, habilitando o operador `$geoNear` do MongoDB — que calcula distâncias em superfície esférica (metros reais, não graus cartesianos).

A pipeline de agregação no `GetByProximityAsync` usa `_collection.Aggregate().GeoNear()` do MongoDB.Driver, passando o ponto de referência como `GeoJsonPoint` com coordenadas tipadas. O resultado é uma lista de `Infectado` ordenada por distância crescente, filtrada pelo raio máximo em metros.

**Repository Pattern para testabilidade**

A camada de repositório (`IInfectadoRepository` / `InfectadoRepository`) isola toda a lógica de acesso ao MongoDB. Os controllers dependem da interface, não da implementação concreta. Isso permite substituir a implementação real por um mock em testes unitários, e usar o banco real via Testcontainers em testes de integração.

**Testes de integração com Testcontainers**

O `InfectadoRepositoryIntegrationTests` sobe um container MongoDB 6.0 temporário para cada execução de testes via `IAsyncLifetime`. O teste `GeoQuery_ReturnsNearby` insere um documento com coordenadas reais de São Paulo e executa `GetByProximityAsync` com raio de 1 km. Se o índice `2dsphere` não estiver criado corretamente, o teste falha. Isso é impossível de validar com mocks.

**Pipeline CI/CD com publicação no GHCR**

O workflow do GitHub Actions executa em dois jobs sequenciais: `build` (restore, build, testes) e `publish-docker` (build da imagem Docker, push para GitHub Container Registry). A publicação usa `GITHUB_TOKEN` — sem necessidade de secrets adicionais para o GHCR. A tag `latest` é publicada a cada push em `main`.

---

## 5. Decisões Técnicas e Aprendizados

### Por que MongoDB e não PostgreSQL com PostGIS para consultas geoespaciais?

PostGIS é tecnicamente mais completo para consultas geoespaciais complexas. A escolha do MongoDB foi intencional: o objetivo era demonstrar a integração nativa do MongoDB.Driver com GeoJSON, que é o padrão de facto para dados geoespaciais em APIs REST. MongoDB Atlas tem suporte nativo a `$geoNear` sem extensões adicionais, e o C# Driver serializa/deserializa GeoJSON automaticamente com os tipos corretos.

### O erro mais sutil: longitude antes de latitude

GeoJSON especifica coordenadas como `[longitude, latitude]` — o inverso da intuição geográfica brasileira (latitude, longitude). Na primeira implementação, as coordenadas foram inseridas invertidas e a query `$geoNear` retornava resultados incorretos. O índice `2dsphere` não valida a ordem — ele confia que o dado está correto. Descobrir esse bug exigiu comparar os resultados com o Google Maps. O aprendizado: sempre validar coordenadas geoespaciais com uma ferramenta visual antes de confiar nos resultados da query.

### Por que Testcontainers e não um banco MongoDB de teste fixo?

Bancos de teste fixos criam acoplamento de infraestrutura: o desenvolvedor precisa ter MongoDB rodando localmente na porta certa. Testcontainers elimina esse acoplamento — o teste sobe e destrói o container automaticamente, com banco limpo a cada execução. O trade-off é tempo de startup (8–15 segundos para subir o container), aceitável para testes de integração que validam comportamento real de banco.

### O desafio da API GeoNear no MongoDB.Driver C#

A documentação oficial do MongoDB.Driver para `$geoNear` via agregação em C# é escassa. A implementação com `_collection.Aggregate().GeoNear()` requer o uso do namespace completo `MongoDB.Driver.GeoJson.GeoJsonPoint<MongoDB.Driver.GeoJson.Core.GeoJson2DGeographicCoordinates>` — qualificação necessária para evitar ambiguidade com outras definições de `GeoJsonPoint` no projeto. Esse detalhe não aparece nos tutoriais padrão e custou algumas horas de debugging.

### O que faria diferente

Extrairia o `GeoJsonPoint` customizado do modelo `Infectado` e usaria diretamente os tipos do `MongoDB.Driver.GeoJson` namespace, evitando a classe customizada que criou o conflito de namespace. Também adicionaria um DTO de saída separado do modelo de domínio, expondo `distanceMeters` no resultado do `nearby` para que o cliente saiba a distância exata de cada resultado.

---

## 6. Resultados

O projeto entregou uma API funcional com os seguintes resultados concretos:

**Consulta geoespacial operacional:** `GET /api/infectado/nearby?lat=-23.56&lon=-46.65&maxKm=5&limit=20` retorna registros ordenados por distância crescente usando `$geoNear` com índice `2dsphere` — consulta esférica real, não aproximação cartesiana.

**Índice `2dsphere` criado automaticamente:** o `InfectadoRepository` garante o índice no startup via `EnsureIndexes()`. Qualquer instância nova do banco tem o índice criado antes da primeira query geoespacial.

**5 endpoints CRUD + 2 endpoints de proximidade:** `/nearby` e `/proximity` expõem o mesmo `GetByProximityAsync` com parâmetros `lat`, `lon`, `maxKm` e `limit` via query string.

**Testes de integração com banco real:** `CreateAndGetById_Works` valida o ciclo completo de inserção e leitura; `GeoQuery_ReturnsNearby` valida que `$geoNear` retorna documentos dentro do raio especificado — ambos contra MongoDB 6.0 real via Testcontainers.

**Imagem Docker publicada automaticamente:** cada push em `main` gera e publica uma imagem no GHCR via GitHub Actions, sem configuração manual de secrets além do `GITHUB_TOKEN` padrão.

---

## 7. Próximos Passos

- Refatorar o `GeoJsonPoint` customizado para usar os tipos nativos do `MongoDB.Driver.GeoJson` namespace, eliminando a necessidade de qualificação de namespace no repositório.
- Adicionar DTO de saída para o endpoint `nearby` expondo o campo `distanceMeters` calculado pelo `$geoNear`.
- Implementar autenticação JWT para proteger os endpoints de criação, atualização e deleção.
- Adicionar FluentValidation para validação de entrada no `InfectadoDto`, substituindo as DataAnnotations atuais.
- Configurar health check para a conexão MongoDB (`/health`) com verificação de conectividade ao banco.
- Expandir os testes de integração para cobrir `GetByProximityAsync` com casos de borda: raio zero, coordenadas no limite dos intervalos válidos, coleção vazia.

---

## Quickstart

### Pré-requisitos

- Docker Desktop
- Git

### Subir com Docker Compose

```bash
git clone https://github.com/Santosdevbjj/proj-Api-MongoDB.git
cd proj-Api-MongoDB

docker-compose up --build

# API:     http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Executar localmente sem Docker

```bash
# Configure appsettings.json com sua connection string
# (local ou MongoDB Atlas)

cd src/projApiMongoDB.Api
dotnet run

# API disponível em: https://localhost:5001
```

### Executar testes

```bash
# Testes unitários e de integração (requer Docker para Testcontainers)
dotnet test ./src/projApiMongoDB.Tests
```

---

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/infectado?page=1&pageSize=20` | Lista com paginação |
| `GET` | `/api/infectado/{id}` | Busca por ObjectId |
| `POST` | `/api/infectado` | Cria registro com coordenadas |
| `PUT` | `/api/infectado/{id}` | Atualiza registro |
| `DELETE` | `/api/infectado/{id}` | Remove registro |
| `GET` | `/api/infectado/nearby?lat=&lon=&maxKm=&limit=` | Busca geoespacial `$geoNear` |
| `GET` | `/api/infectado/proximity?lat=&lon=&maxKm=&limit=` | Alias do nearby |

### Exemplo: criar registro

```json
POST /api/infectado
{
  "dataNascimento": "1990-03-01T00:00:00Z",
  "sexo": "M",
  "latitude": -23.5630994,
  "longitude": -46.6565712
}
```

### Exemplo: busca por proximidade

```bash
# Casos em até 5 km de São Paulo - Av. Paulista
GET /api/infectado/nearby?lat=-23.5630994&lon=-46.6565712&maxKm=5&limit=20
```

---

## Estrutura do Projeto

```
proj-Api-MongoDB/
├── docker-compose.yml                    # Serviços: mongodb + api
├── Dockerfile                            # Build multi-stage .NET 8
├── .github/workflows/dotnet.yml          # CI: build → test → publish GHCR
│
└── src/
    ├── projApiMongoDB.Api/
    │   ├── Program.cs                    # DI, MongoDB, Swagger, controllers
    │   ├── Controllers/
    │   │   └── InfectadoController.cs   # CRUD + nearby + proximity
    │   ├── Models/
    │   │   └── Infectado.cs             # Entidade + GeoJsonPoint
    │   ├── DTOs/
    │   │   └── InfectadoDto.cs          # Entrada com lat/lon validados
    │   ├── Repositories/
    │   │   ├── IInfectadoRepository.cs  # Contrato com GetByProximityAsync
    │   │   └── InfectadoRepository.cs  # $geoNear + índice 2dsphere
    │   └── Settings/
    │       └── MongoDbSettings.cs       # Connection string, DB, collection
    │
    └── projApiMongoDB.Tests/
        ├── InfectadoRepositoryTests.cs              # Unit: construtor + mock
        └── InfectadoRepositoryIntegrationTests.cs  # Integração: Testcontainers
```

---

## Variáveis de Ambiente

| Variável | Exemplo | Descrição |
|---|---|---|
| `MongoDbSettings__ConnectionString` | `mongodb://mongodb:27017` | String de conexão MongoDB |
| `MongoDbSettings__DatabaseName` | `projApiMongoDB` | Nome do banco |
| `MongoDbSettings__InfectadosCollectionName` | `infectados` | Nome da coleção |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Habilita Swagger e exceções detalhadas |

> Para MongoDB Atlas, substitua a connection string pelo URI do Atlas: `mongodb+srv://user:pass@cluster.mongodb.net/`

---

## Tecnologias Utilizadas

| Camada | Tecnologia | Justificativa |
|---|---|---|
| **Runtime** | .NET 8, C# | Performance, nullable safety, integração MongoDB nativa |
| **Banco** | MongoDB 6.0 | GeoJSON nativo, índice 2dsphere, $geoNear em pipeline |
| **Driver** | MongoDB.Driver 2.22 | LINQ, builders tipados, GeoJson namespace |
| **Serialização** | MongoDB.Bson | BsonId, BsonElement, BsonRepresentation |
| **Documentação** | Swagger (Swashbuckle) | UI interativa para testes manuais |
| **Testes de unidade** | xUnit + Moq | Isolamento de construtor e dependências |
| **Testes de integração** | xUnit + Testcontainers | MongoDB real em container por execução |
| **Containers** | Docker + Docker Compose | Paridade dev/prod, MongoDB local sem instalação |
| **CI/CD** | GitHub Actions + GHCR | Build → test → publish automático |

---

## Autor

**Sergio Santos**  
Data Engineer & Cloud Architect — 15+ anos em sistemas críticos bancários (Bradesco)  
Campus Expert DIO · Bootcamp GFT Start #7 .NET

[![Portfólio](https://img.shields.io/badge/Portfólio-Sérgio_Santos-111827?style=for-the-badge&logo=githubpages&logoColor=00eaff)](https://portfoliosantossergio.vercel.app)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Sérgio_Santos-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://linkedin.com/in/santossergioluiz)
[![GitHub](https://img.shields.io/badge/GitHub-Santosdevbjj-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Santosdevbjj)
