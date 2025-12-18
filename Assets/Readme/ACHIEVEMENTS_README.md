# Sistema de Achievements

## Visão Geral

Sistema completo de achievements (conquistas) para o jogo, com editor customizado e integração facilitada.

## Estrutura

### Arquivos Principais

1. **AchievementIds.cs** - Constantes com todos os IDs de achievements
2. **AchievementSystem.cs** - Sistema centralizado para desbloquear achievements
3. **SOAchievement.cs** - ScriptableObject que define um achievement
4. **SOAchievementLibrary.cs** - Biblioteca contendo todos os achievements
5. **ManagerAchievements.cs** - Gerenciador de achievements em runtime
6. **SaveClientAchievements.cs** - Sistema de save/load de achievements

### Ferramentas do Editor

1. **EditorAchievementCentral.cs** - Central de achievements (Window > Tools > Achievements > Central de Achievements)
2. **EditorAchievementCreator.cs** - Criador automático de achievements (Window > Tools > Achievements > Criar Todos os Achievements)

## Como Usar

### 1. Desbloquear um Achievement

```csharp
// Use a constante do AchievementIds
AchievementSystem.Unlock(AchievementIds.FIRST_MATCH_WIN);

// Ou use o ID diretamente (não recomendado)
AchievementSystem.Unlock("achiev_first_win");
```

### 2. Verificar se está Desbloqueado

```csharp
if (AchievementSystem.IsUnlocked(AchievementIds.FIRST_MATCH_WIN))
{
    // Achievement já foi desbloqueado
}
```

### 3. Desbloquear Múltiplos

```csharp
AchievementSystem.UnlockMultiple(
    AchievementIds.FIRST_MATCH_WIN,
    AchievementIds.COMPLETE_TUTORIAL
);
```

## Central de Achievements (Editor)

Acesse via: **Tools > Achievements > Central de Achievements**

A central mostra:
- Todos os achievements do jogo
- Onde cada achievement está sendo disparado (campo `codeLocation`)
- Descrição de como o jogador ganha (campo `editorDescription`)
- Status (com/sem código definido)
- Filtros de busca

### Funcionalidades

- **Buscar**: Filtra por ID, título ou localização do código
- **Filtros**: Mostrar apenas achievements com/sem código definido
- **Estatísticas**: Total de achievements, quantos têm código, quantos não têm
- **Selecionar**: Clica em "Selecionar" para abrir o achievement no Inspector

## Criar Achievements

### Método Automático (Recomendado)

1. Acesse: **Tools > Achievements > Criar Todos os Achievements**
2. Configure a pasta de destino
3. Clique em "Criar Todos os Achievements"
4. Configure manualmente os sprites e chaves de localização

### Método Manual

1. Clique direito no Project
2. **Create > Config > Achievement**
3. Configure:
   - `id`: ID único (use AchievementIds)
   - `titleKey`: Chave de localização do título
   - `descriptionKey`: Chave de localização da descrição
   - `editorDescription`: Descrição de como ganhar (apenas para referência)
   - `codeLocation`: Onde no código este achievement é desbloqueado
   - Sprites (border, icon, lock)

## Integração no Código

### Exemplo: Desbloquear ao ganhar partida

```csharp
// Em ManagerTurn.cs ou similar
void OnMatchFinished(MatchResult result, int playerScore, int opponentScore)
{
    if (result == MatchResult.PlayerWin)
    {
        AchievementSystem.Unlock(AchievementIds.FIRST_MATCH_WIN);
    }
}
```

### Exemplo: Desbloquear ao executar combo

```csharp
// Em RuleCombo.cs
void OnComboTriggered()
{
    AchievementSystem.Unlock(AchievementIds.FIRST_COMBO);
}
```

Veja `AchievementIntegrationExample.cs` para mais exemplos detalhados.

## Lista de Achievements

### Progressão do Jogo
- `FIRST_MATCH_WIN` - Primeira vitória
- `WIN_10_MATCHES` - Ganhe 10 partidas
- `WIN_50_MATCHES` - Ganhe 50 partidas
- `WIN_100_MATCHES` - Ganhe 100 partidas
- `COMPLETE_TUTORIAL` - Complete o tutorial
- `REACH_CITY_2` até `REACH_CITY_7` - Alcance cada cidade
- `COMPLETE_GAME` - Complete o jogo

### Combate e Estratégia
- `PERFECT_VICTORY` - Vitória perfeita
- `COMEBACK_VICTORY` - Vitória vindo de trás
- `WIN_WITH_1_HP` - Vitória com 1 ponto de diferença
- `FIRST_COMBO` - Primeiro combo
- `COMBO_MASTER` - 10 combos
- `CAPTURE_5_CARDS` / `CAPTURE_10_CARDS` - Capture X cartas
- `WIN_IN_5_TURNS` / `WIN_IN_3_TURNS` - Vitória rápida

### Cartas e Coleção
- `COLLECT_10_CARDS` / `COLLECT_50_CARDS` / `COLLECT_100_CARDS` - Colete X cartas
- `COLLECT_ALL_CARDS` - Colete todas as cartas
- `PLAY_LEGENDARY` - Jogue uma carta lendária
- `COLLECT_5_LEGENDARIES` - Colete 5 lendárias

### Exploração e Mundo
- `INTERACT_FIRST` - Primeira interação
- `VISIT_ALL_AREAS` - Visite todas as áreas
- `COMPLETE_ALL_DIALOGUES` - Complete todos os diálogos
- `USE_PHONE_BANK` / `USE_PHONE_ZAP` / `USE_PHONE_DRIVE` - Use apps do telefone

### Especiais e Desafios
- `WIN_STREAK_5` / `WIN_STREAK_10` - Sequência de vitórias
- `NO_CAPTURES_WIN` - Vitória sem capturas
- `ALL_SAME_ELEMENT` - Vitória com mesmo elemento
- `FIRST_DRAW` - Primeiro empate
- `SPEEDRUN_30MIN` - Complete em menos de 30 minutos

## Boas Práticas

1. **Sempre use constantes**: Use `AchievementIds.FIRST_MATCH_WIN` ao invés de `"achiev_first_win"`
2. **Atualize codeLocation**: Sempre preencha o campo `codeLocation` no SOAchievement quando integrar
3. **Verifique antes de desbloquear**: Use `IsUnlocked()` para evitar desbloquear novamente (opcional, o sistema já faz isso)
4. **Use a Central**: Regularmente verifique a Central de Achievements para ver quais ainda não têm código

## Troubleshooting

### Achievement não está sendo desbloqueado
- Verifique se o `SaveClientAchievements` está no scene
- Verifique se o ID está correto (use AchievementIds)
- Verifique os logs do Unity para erros

### Central não mostra achievements
- Verifique se existe um `SOAchievementLibrary` no projeto
- Recarregue a biblioteca clicando em "Recarregar"

### Achievements não estão salvando
- Verifique se o `SOSaveDefinition` está configurado no `SaveClientAchievements`
- Verifique se o `ManagerSave` está funcionando corretamente
