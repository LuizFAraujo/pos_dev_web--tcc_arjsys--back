<!-- markdownlint-disable-file -->
# ⚙️ CONFIG: Move .slnx para raiz e cria tasks do VS Code

## 💡 Por que mover o .slnx

O padrão recomendado é a solution (`.slnx`) ficar na raiz, acima dos projetos. Isso evita conflitos e facilita comandos do `dotnet` CLI.

### Estrutura atualizada

```
raiz/
├── .vscode/
│   ├── settings.json
│   └── tasks.json
├── app/
│   ├── Api_ArjSys_Tcc.csproj
│   ├── Program.cs
│   └── ...
├── docs/
├── Api_ArjSys_Tcc.slnx
├── .gitignore
└── README.md
```

### Como rodar a API

Da raiz:

```bash
dotnet run --project app
dotnet watch --project app
dotnet build --project app
```

Ou via VS Code: `Ctrl+Shift+P` → "Run Task" → escolher ▶ Run API, 👁 Watch API ou 🔨 Build API.

---

## 📁 Arquivos criados

| Arquivo | Função |
|---------|--------|
| `.vscode/tasks.json` | Atalhos Run, Watch e Build no VS Code |

## 📝 Arquivos alterados

| Arquivo | Alteração |
|---------|-----------|
| `Api_ArjSys_Tcc.slnx` | Movido para raiz, path atualizado para `app/Api_ArjSys_Tcc.csproj` |

