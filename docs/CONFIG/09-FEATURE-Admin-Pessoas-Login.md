# ✨ FEATURE: Implementa módulo Admin (Pessoas, Clientes, Funcionários, Permissões, Login)

## 💡 Conceitos aplicados

### Pessoa como base compartilhada

Pessoa é a classe base com dados comuns (nome, CPF/CNPJ, telefone, email, endereço). Cliente e Funcionário referenciam Pessoa via FK. O enum TipoPessoa (Cliente, Funcionario, Ambos) indica o papel da pessoa no sistema.

Ao criar um Cliente, o service cria automaticamente a Pessoa base + o registro Cliente. Ao excluir, remove ambos. O DTO de criação recebe tudo junto (dados da pessoa + dados específicos do cliente/funcionário), mas internamente são 2 tabelas.

### Hash de senha

Senha armazenada como hash SHA256. O service nunca armazena a senha em texto puro. O ResponseDTO do funcionário **nunca retorna a senha**. No Update, a senha só é re-hashada se um valor novo for informado (campo não vazio).

### Login simples

O AuthService recebe usuário + senha, busca o funcionário pelo usuário, compara o hash, e retorna os dados do funcionário com suas permissões. Sem JWT por enquanto — o frontend vai armazenar os dados localmente. JWT pode ser adicionado depois.

### Permissões por módulo

Cada funcionário pode ter uma permissão por módulo do sistema. Os níveis são: SemAcesso, Leitura, LeituraEscrita, Admin. Os módulos são: Engenharia, Comercial, PCP, Compras, Almoxarifado, Admin.

---

## 📁 Arquivos criados

| Arquivo | Função |
|---------|--------|
| `Models/Admin/Enums/TipoPessoa.cs` | Enum: Cliente, Funcionario, Ambos |
| `Models/Admin/Enums/NivelAcesso.cs` | Enum: SemAcesso, Leitura, LeituraEscrita, Admin |
| `Models/Admin/Enums/ModuloSistema.cs` | Enum: Engenharia, Comercial, PCP, Compras, Almoxarifado, Admin |
| `Models/Admin/Pessoa.cs` | Entidade base (herda BaseEntity) |
| `Models/Admin/Cliente.cs` | FK para Pessoa + RazaoSocial, IE, ContatoComercial |
| `Models/Admin/Funcionario.cs` | FK para Pessoa + Cargo, Setor, Usuario, SenhaHash |
| `Models/Admin/Permissao.cs` | FK para Funcionario + Modulo + Nivel |
| `DTOs/Admin/ClienteDTO.cs` | ClienteCreateDTO e ClienteResponseDTO |
| `DTOs/Admin/FuncionarioDTO.cs` | FuncionarioCreateDTO e FuncionarioResponseDTO |
| `DTOs/Admin/PermissaoDTO.cs` | PermissaoCreateDTO e PermissaoResponseDTO |
| `DTOs/Admin/LoginDTO.cs` | LoginRequestDTO e LoginResponseDTO |
| `Data/Configurations/Admin/PessoaConfiguration.cs` | ToTable Admin_Pessoas, MaxLengths |
| `Data/Configurations/Admin/ClienteConfiguration.cs` | ToTable Admin_Clientes, FK Restrict |
| `Data/Configurations/Admin/FuncionarioConfiguration.cs` | ToTable Admin_Funcionarios, FK Restrict, índice único Usuario |
| `Data/Configurations/Admin/PermissaoConfiguration.cs` | ToTable Admin_Permissoes, índice único Func+Modulo, Cascade |
| `Services/Admin/ClienteService.cs` | CRUD + cria Pessoa base automaticamente |
| `Services/Admin/FuncionarioService.cs` | CRUD + hash senha SHA256 + validação usuário único |
| `Services/Admin/PermissaoService.cs` | CRUD + validação duplicidade Func+Modulo |
| `Services/Admin/AuthService.cs` | Login simples: valida usuario+senha, retorna dados+permissões |
| `Controllers/Admin/ClientesController.cs` | Endpoints CRUD clientes |
| `Controllers/Admin/FuncionariosController.cs` | Endpoints CRUD funcionários |
| `Controllers/Admin/PermissoesController.cs` | Endpoints CRUD permissões |
| `Controllers/Admin/AuthController.cs` | POST login |

## 📝 Arquivos alterados

| Arquivo | Alteração |
|---------|-----------|
| `Data/AppDbContext.cs` | Adicionado DbSets: Pessoas, Clientes, Funcionarios, Permissoes |
| `Program.cs` | Registrado ClienteService, FuncionarioService, PermissaoService, AuthService |

---

## 🗄️ Tabelas no banco

| Tabela | Prefixo | Registros na carga |
|--------|---------|-------------------|
| `Admin_Pessoas` | Admin_ | 25 (15 clientes + 10 funcionários) |
| `Admin_Clientes` | Admin_ | 15 |
| `Admin_Funcionarios` | Admin_ | 10 |
| `Admin_Permissoes` | Admin_ | 38 |

---

## 🌐 Endpoints

### Admin - Clientes

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/admin/Clientes` | Lista todos os clientes |
| GET | `/api/admin/Clientes/{id}` | Busca cliente por ID |
| POST | `/api/admin/Clientes` | Cria cliente (+ Pessoa base) |
| PUT | `/api/admin/Clientes/{id}` | Atualiza cliente (+ Pessoa base) |
| DELETE | `/api/admin/Clientes/{id}` | Remove cliente (+ Pessoa base) |

### Admin - Funcionários

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/admin/Funcionarios` | Lista todos os funcionários |
| GET | `/api/admin/Funcionarios/{id}` | Busca funcionário por ID |
| POST | `/api/admin/Funcionarios` | Cria funcionário (+ Pessoa base, hash senha) |
| PUT | `/api/admin/Funcionarios/{id}` | Atualiza funcionário (re-hash senha se informada) |
| DELETE | `/api/admin/Funcionarios/{id}` | Remove funcionário (+ Pessoa base) |

### Admin - Permissões

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/admin/Permissoes/funcionario/{funcId}` | Lista permissões de um funcionário |
| POST | `/api/admin/Permissoes` | Cria permissão (valida duplicidade Func+Módulo) |
| PUT | `/api/admin/Permissoes/{id}` | Atualiza nível de permissão |
| DELETE | `/api/admin/Permissoes/{id}` | Remove permissão |

### Admin - Auth

| Método | Rota | Ação |
|--------|------|------|
| POST | `/api/admin/Auth/login` | Login (retorna dados do funcionário + permissões) |

---

## ✅ Validações implementadas

- Nome obrigatório ao criar Cliente ou Funcionário
- Usuário obrigatório e único ao criar Funcionário
- Senha obrigatória ao criar, opcional ao atualizar (só re-hash se informada)
- Permissão: não permite duplicar Funcionário + Módulo
- Permissão: valida existência do Funcionário
- Login: valida usuário e senha, verifica se está ativo
- Exclusão: remove Pessoa base junto com Cliente/Funcionário

---

## 🧪 Roteiro de Testes

### CLIENTES

| # | Método | Rota | Body/Params | Esperado |
|---|--------|------|-------------|----------|
| 1 | GET | `/api/admin/Clientes` | — | ✅ 200 lista 15 clientes |
| 2 | GET | `/api/admin/Clientes/1` | — | ✅ 200 Metalúrgica São José |
| 3 | GET | `/api/admin/Clientes/999` | — | ✅ 404 |
| 4 | POST | `/api/admin/Clientes` | ver abaixo | ✅ 201 |
| 5 | PUT | `/api/admin/Clientes/{id}` | ver abaixo | ✅ 204 |
| 6 | DELETE | `/api/admin/Clientes/{id}` | — | ✅ 204 |
| 7 | POST | `/api/admin/Clientes` | sem nome | ❌ 400 "Nome é obrigatório" |

**Teste 4 — POST criar cliente:**
```json
{
  "nome": "Cliente Teste Ltda",
  "cpfCnpj": "99.999.999/0001-99",
  "telefone": "(44) 3399-9999",
  "email": "teste@teste.com.br",
  "endereco": "Rua Teste 100",
  "cidade": "Maringá",
  "estado": "PR",
  "cep": "87000-000",
  "razaoSocial": "Cliente Teste Ltda EPP",
  "inscricaoEstadual": "999.99999-99",
  "contatoComercial": "Teste - (44) 99999-9999"
}
```

**Teste 5 — PUT atualizar (usar id retornado no POST):**
```json
{
  "nome": "Cliente Teste ATUALIZADO",
  "cpfCnpj": "99.999.999/0001-99",
  "telefone": "(44) 3399-8888",
  "email": "atualizado@teste.com.br",
  "cidade": "Londrina",
  "estado": "PR",
  "razaoSocial": "Cliente Teste Atualizado Ltda"
}
```

**Teste 7 — POST sem nome:**
```json
{
  "nome": "",
  "razaoSocial": "Sem Nome Ltda"
}
```

---

### FUNCIONÁRIOS

| # | Método | Rota | Body/Params | Esperado |
|---|--------|------|-------------|----------|
| 1 | GET | `/api/admin/Funcionarios` | — | ✅ 200 lista 10 funcionários |
| 2 | GET | `/api/admin/Funcionarios/1` | — | ✅ 200 João Carlos Silva |
| 3 | GET | `/api/admin/Funcionarios/999` | — | ✅ 404 |
| 4 | POST | `/api/admin/Funcionarios` | ver abaixo | ✅ 201 |
| 5 | POST | `/api/admin/Funcionarios` | usuário duplicado | ❌ 400 "já está em uso" |
| 6 | POST | `/api/admin/Funcionarios` | sem usuário | ❌ 400 "Usuário é obrigatório" |
| 7 | POST | `/api/admin/Funcionarios` | sem senha | ❌ 400 "Senha é obrigatória" |
| 8 | PUT | `/api/admin/Funcionarios/{id}` | ver abaixo | ✅ 204 |
| 9 | DELETE | `/api/admin/Funcionarios/{id}` | — | ✅ 204 |

**Teste 4 — POST criar funcionário:**
```json
{
  "nome": "Funcionário Teste",
  "cpfCnpj": "111.222.333-44",
  "telefone": "(44) 99999-0000",
  "email": "func.teste@email.com",
  "cargo": "Auxiliar",
  "setor": "TI",
  "usuario": "func.teste",
  "senha": "teste123"
}
```

**Teste 5 — POST usuário duplicado (repetir usuario "func.teste"):**
```json
{
  "nome": "Outro Funcionário",
  "usuario": "func.teste",
  "senha": "outra123"
}
```

**Teste 8 — PUT atualizar (usar id retornado no teste 4):**
```json
{
  "nome": "Funcionário Teste ATUALIZADO",
  "cargo": "Analista",
  "setor": "Engenharia",
  "usuario": "func.teste",
  "senha": "novasenha456"
}
```

---

### PERMISSÕES

| # | Método | Rota | Body/Params | Esperado |
|---|--------|------|-------------|----------|
| 1 | GET | `/api/admin/Permissoes/funcionario/1` | — | ✅ 200 lista 6 permissões (Diretor = Admin em tudo) |
| 2 | GET | `/api/admin/Permissoes/funcionario/2` | — | ✅ 200 lista 4 permissões (Gerente Comercial) |
| 3 | GET | `/api/admin/Permissoes/funcionario/999` | — | ✅ 200 lista vazia [] |
| 4 | POST | `/api/admin/Permissoes` | ver abaixo | ✅ 200 permissão criada |
| 5 | POST | `/api/admin/Permissoes` | duplicado | ❌ 400 "Já existe permissão" |
| 6 | POST | `/api/admin/Permissoes` | func inexistente | ❌ 400 "Funcionário não encontrado" |
| 7 | PUT | `/api/admin/Permissoes/{id}` | ver abaixo | ✅ 204 |
| 8 | DELETE | `/api/admin/Permissoes/{id}` | — | ✅ 204 |

**Teste 4 — POST criar permissão (Fernanda, id=6, acesso Leitura em Engenharia):**
```json
{
  "funcionarioId": 6,
  "modulo": "Engenharia",
  "nivel": "Leitura"
}
```

**Teste 5 — POST duplicado (repetir mesmo body):**
```json
{
  "funcionarioId": 6,
  "modulo": "Engenharia",
  "nivel": "Leitura"
}
```

**Teste 6 — POST funcionário inexistente:**
```json
{
  "funcionarioId": 999,
  "modulo": "Engenharia",
  "nivel": "Leitura"
}
```

**Teste 7 — PUT alterar nível (usar id retornado no teste 4):**
```json
{
  "funcionarioId": 6,
  "modulo": "Engenharia",
  "nivel": "LeituraEscrita"
}
```

---

### AUTH (LOGIN)

| # | Método | Rota | Body/Params | Esperado |
|---|--------|------|-------------|----------|
| 1 | POST | `/api/admin/Auth/login` | login válido | ✅ 200 dados + permissões |
| 2 | POST | `/api/admin/Auth/login` | senha errada | ❌ 401 "Usuário ou senha inválidos" |
| 3 | POST | `/api/admin/Auth/login` | usuário inexistente | ❌ 401 "Usuário ou senha inválidos" |
| 4 | POST | `/api/admin/Auth/login` | campos vazios | ❌ 401 "obrigatórios" |

**Teste 1 — Login válido (Diretor, todas permissões):**
```json
{
  "usuario": "joao.silva",
  "senha": "arjsys123"
}
```

**Teste 2 — Senha errada:**
```json
{
  "usuario": "joao.silva",
  "senha": "senhaerrada"
}
```

**Teste 3 — Usuário inexistente:**
```json
{
  "usuario": "naoexiste",
  "senha": "qualquer"
}
```

**Teste 4 — Campos vazios:**
```json
{
  "usuario": "",
  "senha": ""
}
```

---

## 📊 Resumo de Testes

| Módulo | Total |
|--------|-------|
| Clientes | 7 |
| Funcionários | 9 |
| Permissões | 8 |
| Auth | 4 |
| **Total Fase 2** | **28** |

---

## 🔐 Funcionários de teste e senhas

Todos os funcionários da carga inicial usam a senha: **arjsys123**

| Usuário | Cargo | Perfil de acesso |
|---------|-------|------------------|
| joao.silva | Diretor | Admin em todos os módulos |
| maria.souza | Gerente Comercial | LeituraEscrita Comercial |
| carlos.lima | Engenheiro | LeituraEscrita Engenharia |
| ana.oliveira | Analista PCP | LeituraEscrita PCP |
| roberto.santos | Comprador | LeituraEscrita Compras |
| fernanda.costa | Almoxarife | LeituraEscrita Almoxarifado |
| pedro.martins | Operador | Leitura PCP/Engenharia |
| lucas.pereira | Auxiliar | Leitura PCP |
| juliana.almeida | Vendedora | LeituraEscrita Comercial |
| marcos.rocha | Admin TI | Admin em todos os módulos |

---

