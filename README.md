# Supernova Colheita

**API RESTful — Global Solution 2026/2 | CitizenTech | FIAP**

**Disciplina:** Advanced Business Development with .NET  
**Repositório:** https://github.com/kaique-masc/SUPERNOVACOLHEITA_DOTNET

## 1. Contexto e objetivo

O Supernova Colheita é uma proposta de serviço digital para apoiar pequenos produtores rurais. A solução reúne cadastro de produtores, registro de produtos agrícolas, divulgação de oportunidades e consulta de condições climáticas em uma API que pode ser consumida por aplicativos.

A proposta se relaciona ao tema **CitizenTech** por buscar ampliar o acesso a informações e ferramentas digitais úteis à organização da atividade rural. O projeto pretende reduzir a dispersão de informações e facilitar o planejamento e a visibilidade da produção. Esses benefícios são objetivos da solução, não resultados de impacto já medidos.

## 2. Funcionalidades implementadas

- Cadastro de usuários e login com autenticação JWT.
- Consulta, cadastro e exclusão de produtores.
- Consulta, cadastro e exclusão de produtos.
- Consulta, cadastro e exclusão de oportunidades.
- Consulta de informações climáticas por latitude e longitude, com dados da API Open-Meteo.
- Idempotência no cadastro de produtores para evitar registros duplicados em reenvios.
- Limitação de requisições em rotas protegidas, com resposta HTTP 429 e limite configurável.
- Aceita o token JWT emitido pela API Java (login único no app) e o token do login próprio desta API.
- CORS liberado para o app rodando no navegador (versão web do Expo).
- Oportunidades de exemplo (seed) para o app não abrir vazio.
- Documentação interativa Swagger e verificações de saúde da aplicação.

**Escopo atual:** os recursos de produtores, produtos e oportunidades expõem `GET`, `POST` e `DELETE`; não há endpoint `PUT`/`PATCH` documentado nesta versão.

### Uso pelo app mobile

O usuário faz login uma única vez, na API Java. Esta API valida o mesmo token JWT (mesma chave e issuer da Java aceito) e também continua aceitando o token do próprio `/api/auth/login`.

| Tela no app | Endpoint | Quem usa |
|---|---|---|
| Oportunidades (chamadas públicas abertas) | `GET /api/oportunidades`, `GET /api/oportunidades/{id}` | Produtor |
| Publicar oportunidade | `POST /api/oportunidades`, `DELETE /api/oportunidades/{id}` | Comprador (escola) e gestor |
| Clima da lavoura | `GET /api/clima?latitude=&longitude=` | Produtor |
| Status da API ("Sobre o App") | `GET /health` | Todos |

`/api/produtores` e `/api/produtos` ficam disponíveis só no Swagger: o app usa os equivalentes da API Java, para não duplicar dados. `POST` e `DELETE` de oportunidades exigem o perfil **COMPRADOR** ou **GESTOR** (os demais perfis recebem `HTTP 403`). A leitura de oportunidades, o clima e as demais rotas aceitam qualquer usuário autenticado.

## 3. Tecnologias

| Componente | Tecnologia |
|---|---|
| Linguagem e plataforma | C# / .NET 8 |
| API | ASP.NET Core Minimal APIs |
| Persistência | Entity Framework Core / SQLite |
| Segurança | JWT Bearer e hash de senhas com ASP.NET Core Identity PasswordHasher |
| Documentação | Swagger / OpenAPI (Swashbuckle) |
| Proteção de tráfego | ASP.NET Core Rate Limiting |
| Testes | xUnit e testes de integração com WebApplicationFactory |
| Monitoramento | ASP.NET Core Health Checks |
| Serviço externo | Open-Meteo |

## 4. Estrutura do repositório

```text
SupernovaColheita/
├── SupernovaColheita.Api/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── DadosIniciais.cs
│   ├── Endpoints/
│   │   ├── AuthEndpoints.cs
│   │   ├── ClimaEndpoints.cs
│   │   ├── OportunidadeEndpoints.cs
│   │   ├── ProdutoEndpoints.cs
│   │   └── ProdutorEndpoints.cs
│   ├── HealthChecks/
│   │   └── OpenMeteoHealthCheck.cs
│   ├── Models/
│   ├── Services/
│   ├── Program.cs
│   └── appsettings.json
├── SupernovaColheita.Tests/
├── SupernovaColheita.sln
└── README.md
```

## 5. Arquitetura e fluxo

```text
Aplicativo cliente / Swagger
             |
             v
    ASP.NET Core Minimal API
             |
    +--------+--------------------+
    |        |                    |
Autenticação Rate Limiting  Endpoints e validações
JWT          |                    |
             |          +---------+----------+
             |          |                    |
             |      EF Core             ClimaService
             |          |                    |
             |        SQLite              Open-Meteo
             |
             +-- /health: banco de dados e serviço externo
```

As requisições chegam aos endpoints HTTP, passam pelos middlewares de autenticação, limitação de requisições e autorização, conforme a configuração de cada rota. Os dados persistentes são acessados via Entity Framework Core e SQLite. A consulta de clima utiliza um serviço HTTP externo.

## 6. Pré-requisitos

- SDK do .NET 8 instalado.
- Acesso à internet para restaurar os pacotes NuGet e consultar o Open-Meteo.
- Git, caso deseje clonar o repositório.

## 7. Como executar

Clone o projeto e entre na pasta principal:

```bash
git clone https://github.com/kaique-masc/SUPERNOVACOLHEITA_DOTNET.git
cd SUPERNOVACOLHEITA_DOTNET
```

Restaure as dependências:

```bash
dotnet restore
```

Execute a API em ambiente de desenvolvimento.

**PowerShell (Windows):**

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project .\SupernovaColheita.Api\SupernovaColheita.Api.csproj
```

**Linux/macOS:**

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project ./SupernovaColheita.Api/SupernovaColheita.Api.csproj
```

**Chave JWT.** Para o app funcionar com o login único, a chave desta API precisa ser **a mesma do `JWT_SECRET` da API Java**, sempre por variável de ambiente (nunca no código nem no `appsettings.json`). No Azure, as duas APIs recebem o mesmo segredo.

```powershell
$env:Jwt__Key = "<mesmo valor do JWT_SECRET da API Java>"
dotnet run --project .\SupernovaColheita.Api\SupernovaColheita.Api.csproj
```

Em `Development`, se `Jwt__Key` não estiver definida, a aplicação gera uma chave temporária em memória e avisa no console. Nesse caso o login próprio desta API funciona, mas **tokens da API Java não são aceitos**, e ao reiniciar a API os tokens anteriores deixam de valer. Fora de `Development`, a chave é obrigatória e precisa ter pelo menos 32 bytes.

**Limite de requisições.** O padrão é 60 por minuto (`RateLimit:PermitLimit` no `appsettings.json`). Para demonstrar o HTTP 429, reduza o limite antes de subir a API:

```powershell
$env:RateLimit__PermitLimit = "5"
```

**Acesso pelo celular.** Para o aparelho alcançar a API na rede local, escute em todas as interfaces:

```bash
dotnet run --project ./SupernovaColheita.Api/SupernovaColheita.Api.csproj --urls http://0.0.0.0:5256
```

O endereço local padrão é `http://localhost:5256`. A porta pode variar conforme o ambiente e as configurações de inicialização.

- **Swagger (Development):** http://localhost:5256/swagger/index.html
- **Health Check:** http://localhost:5256/health

O SQLite utiliza a conexão `Data Source=supernova.db`, configurada em `appsettings.json`. A aplicação cria a estrutura inicial do banco quando necessário. O arquivo de banco local não deve ser versionado.

## 8. Autenticação

O cadastro e o login são públicos. As rotas de negócio são protegidas por JWT.

1. Cadastre um usuário com `POST /api/auth/cadastro`.
2. Autentique-se com `POST /api/auth/login`.
3. Copie o token JWT retornado.
4. Nas chamadas protegidas, envie o cabeçalho:

```http
Authorization: Bearer SEU_TOKEN_JWT
```

No Swagger, utilize **Authorize** e informe `Bearer` seguido do token, conforme o esquema de segurança configurado na versão atual.

**Token da API Java.** O usuário faz login apenas na API Java; o mesmo token funciona aqui, desde que `Jwt__Key` seja igual ao `JWT_SECRET` da Java. Os emissores (issuers) aceitos ficam em `Jwt:Issuers` no `appsettings.json`: `SupernovaColheita` (token desta API) e `supernova-colheita-api` (token da Java). A audiência não é validada, porque a API Java não a inclui no token; a segurança continua pela assinatura (chave secreta), pelo emissor e pela expiração.

As senhas do login próprio são armazenadas como hash, não em texto puro.

**Limitação atual:** a autenticação protege as rotas, mas o controle de propriedade dos registros e as permissões por perfil ainda podem ser aprofundados.

## 9. Endpoints

| Método | Rota | Finalidade | Autenticação |
|---|---|---|---|
| `POST` | `/api/auth/cadastro` | Criar usuário | Não |
| `POST` | `/api/auth/login` | Obter token JWT | Não |
| `GET` | `/api/produtores` | Listar produtores | Sim |
| `GET` | `/api/produtores/{id}` | Consultar produtor | Sim |
| `POST` | `/api/produtores` | Cadastrar produtor | Sim |
| `DELETE` | `/api/produtores/{id}` | Excluir produtor | Sim |
| `GET` | `/api/produtos` | Listar produtos | Sim |
| `GET` | `/api/produtos/{id}` | Consultar produto | Sim |
| `POST` | `/api/produtos` | Cadastrar produto | Sim |
| `DELETE` | `/api/produtos/{id}` | Excluir produto | Sim |
| `GET` | `/api/oportunidades` | Listar oportunidades | Sim |
| `GET` | `/api/oportunidades/{id}` | Consultar oportunidade | Sim |
| `POST` | `/api/oportunidades` | Cadastrar oportunidade | Sim |
| `DELETE` | `/api/oportunidades/{id}` | Excluir oportunidade | Sim |
| `GET` | `/api/clima?latitude=-23.55&longitude=-46.63` | Consultar clima | Sim |
| `GET` | `/health` | Verificar saúde da aplicação | Não |

Os contratos completos de entrada e saída e os códigos HTTP documentados podem ser consultados na interface Swagger. Entre os retornos implementados estão `200`, `201`, `400`, `401`, `404`, `409`, `429` e `503`, conforme o endpoint e a situação.

## 10. Idempotência

O endpoint `POST /api/produtores` utiliza o cabeçalho `Idempotency-Key`:

```http
Idempotency-Key: identificador-unico-da-requisicao
```

Ao repetir uma requisição com a mesma chave e o mesmo conteúdo, a API reutiliza o resultado já registrado, em vez de criar outro produtor. Reutilizar a mesma chave com conteúdo diferente resulta em `HTTP 409 Conflict`.

**Limitações atuais:** a idempotência foi implementada especificamente no cadastro de produtores; a chave não é isolada por usuário e a concorrência simultânea ainda merece testes adicionais.

## 11. Rate Limiting

As rotas protegidas utilizam uma janela fixa de **60 requisições por minuto** (padrão) por usuário autenticado. O usuário é identificado pela claim `id` (token da API Java); na falta dela, pelo identificador do login próprio e, sem token válido, pelo IP. Ao ultrapassar o limite, a API responde com `HTTP 429 Too Many Requests`. Cadastro e login não utilizam essa política na versão atual.

O limite é configurável em `RateLimit:PermitLimit` (`appsettings.json`) ou pela variável de ambiente `RateLimit__PermitLimit`. Um valor baixo (por exemplo, 5) serve para mostrar o 429 na apresentação; o valor padrão evita que o app receba 429 apenas por abrir duas telas.

## 12. Health Checks

O endpoint público `GET /health` verifica a saúde da aplicação, incluindo:

- Acesso ao banco SQLite por meio do Entity Framework Core.
- Disponibilidade do serviço externo Open-Meteo.

Em condição normal, o endpoint retorna `Healthy` com `HTTP 200`. Uma indisponibilidade do serviço externo pode afetar o estado geral de saúde, mesmo que os endpoints locais continuem disponíveis.

## 13. Testes automatizados

Execute na raiz do repositório:

```bash
dotnet test ./SupernovaColheita.Tests/SupernovaColheita.Tests.csproj
```

A suíte inclui verificações de validação dos modelos, autenticação e acesso não autorizado, aceitação do token da API Java (e rejeição de issuer desconhecido ou chave diferente), idempotência, rate limiting (limite padrão e limite reduzido por configuração), CORS e o seed de oportunidades. Ainda é possível ampliar a cobertura para produtos, clima, health checks e documentação Swagger.

**Windows:** se ocorrer erro `MSB3021` ou `MSB3027` por arquivo executável bloqueado, encerre a instância da API antes de executar os testes novamente.

## 14. Decisões técnicas

- **Minimal APIs:** estrutura direta para expor os recursos HTTP e suas validações.
- **EF Core com SQLite:** persistência local simples e reproduzível, sem servidor de banco separado.
- **JWT:** autenticação sem sessão de servidor. A chave é compartilhada com a API Java por variável de ambiente, e a lista de emissores aceitos fica em configuração, para o app usar um único login.
- **Idempotência:** proteção contra cadastros duplicados causados por reenvio de requisições.
- **Rate Limiting:** controle de excesso de chamadas, com limite configurável para uso real (60/min) e para demonstrar o HTTP 429.
- **CORS:** política aberta (qualquer origem), pensada para a versão web do app nos testes e na demonstração.
- **Health Checks:** monitoramento de dependências essenciais e externas.
- **Open-Meteo:** consulta de informações meteorológicas para apoiar decisões do produtor.

## 15. Limitações e próximos passos

- Implementar verificação de propriedade dos registros: hoje só as oportunidades têm restrição por perfil, e qualquer usuário autenticado ainda exclui produtores e produtos.
- Validar com as duas APIs reais no ar (login na Java e chamada à .NET com o token emitido): os testes automatizados usam tokens gerados no formato da Java, o que não substitui esse teste.
- Restringir o CORS às origens do app em produção.
- Fortalecer a idempotência em cenários concorrentes e, se necessário, em outros endpoints.
- Ampliar os testes de integração para os demais recursos.
- Revisar a documentação Swagger/OpenAPI e o esquema de segurança Bearer.
- Reavaliar o limite de requisições e o comportamento dos health checks em um ambiente de produção.
- Planejar hospedagem, armazenamento persistente e gestão de segredos para implantação pública.

## 16. Integrantes

| Nome completo | RM | Turma |
|---|---|---|
| Kaique Mascarenhas dos Santos | RM565802 | A preencher |
| Felipe Augusto Lopes Ferreira | RM563982 | A preencher |

---

**Entrega:** Global Solution 2026/2 — Advanced Business Development with .NET — FIAP.
