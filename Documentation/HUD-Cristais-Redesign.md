# HUD dos cristais — apresentação guiada

## Mudanças

O painel da área de cristais agora apresenta três etapas: **Coletar → Encaixar → Lembrar**. Fundo azul escuro, detalhes dourados, cartões com formas geométricas e textos de ação substituem a lista de controles simultâneos.

- Na coleta, o botão informa o cristal selecionado. A extração mantém seus três segundos, barra e percentual.
- Após cada coleta, o próximo cristal disponível é selecionado automaticamente. Cristais guardados recebem indicação própria.
- Os encaixes aparecem depois dos três cristais coletados. Selecione uma forma e clique na pista correspondente; erros preservam o inventário.
- Encaixes corretos exibem a forma recebida. A seleção avança para o próximo cristal pendente.
- A conclusão revela a lembrança e orienta a conversar com o estranho sobre a tempestade.
- Antes da primeira lembrança, o painel orienta a resolver o pergaminho da pedra central.

Moedas e timer de missão continuam removidos. A duração de três segundos é apenas o tempo da extração. O terceiro puzzle e o final permanecem para uma próxima etapa.

## Arquivos criados

- `Assets/Scripts/CrystalSymbolGraphic.cs` e `.meta`: ícones geométricos desenhados pela UI, sem imagens externas ou emojis.
- `Tests/UIAuthoring/RedesignCrystalHUD.cs`: registro da autoria do painel em cópia isolada; não precisa ser executado novamente na cena pronta.
- Este relatório e capturas de validação.

## Arquivos modificados

- `Assets/Scripts/MiningUI.cs`: apresentação por etapas, seleção automática, instruções contextuais e estados dos cartões/encaixes.
- `Assets/Scenes/Island.unity`: layout e referências do painel existente. Preservados os demais objetos da cena.
- `Tests/GameplayValidation/CrystalPuzzleValidation.cs`: validações do fluxo guiado, alcance dos cliques, textos e capturas.

## Configuração no Unity

Nenhuma configuração manual adicional: referências dos grupos, textos, marcadores e símbolos já foram atribuídas na cena Island. Os callbacks dos botões existentes foram preservados. Abra a cena salva no projeto para carregar o novo layout.

## Assets

Nenhum asset adicional é necessário para o HUD. A animação de mineração do personagem continua dependendo dos clips/controller que já estavam faltando; isso não impede o puzzle.

## Validação

- 66 verificações do HUD/puzzle aprovadas em Play Mode: pré-requisitos, etapas, seleção automática, cliques por raycast, textos, extração, pausa, cancelamento, encaixes, persistência e reinício/menu.
- Capturas do Canvas em 1280 × 720 inspecionadas: bloqueado, coleta, extração, encaixes, erro e lembrança. Corrigidos ícones sem CanvasRenderer e textos que extrapolavam seus cartões.
- Compilação sem erros de código. Persistem avisos de APIs obsoletas e uma ocorrência conhecida do índice de busca do Editor, registrada separadamente do gameplay.
- O teste usa invocação de botões e raycasts da UI em uma cópia isolada; não substitui uma sessão manual de navegação na ilha. Outras proporções de tela ainda precisam de conferência manual.
- Resultado: CrystalHUDValidationResults.txt. Capturas: crystal-hud-collect.png, crystal-hud-fit.png e crystal-hud-complete.png (além dos estados intermediários).

- Regressão do primeiro puzzle: 34 verificações aprovadas. Resultado: FirstPuzzleRegressionAfterHUD.txt.
