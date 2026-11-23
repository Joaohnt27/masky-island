# Botões para pular apresentações

Botão no canto inferior direito, verde-petróleo com borda dourada, texto claro em DynaPuff e indicação de Esc. Clique ou Esc pula a apresentação. O controle fica oculto fora das apresentações e desativa após ser acionado.

- Introdução após confirmar o nome: segue para o menu.
- Vídeo após Jogar: segue para a praia, preservando o início da história.
- Encerramento após vencer: segue para os créditos.

## Arquivos
- Novo: `Assets/Scripts/CutsceneSkipButton.cs` e `.meta`.
- Modificados: `MainMenuController.cs`, `EndingCutscene.cs`, cenas `MainMenu` e `Ending`.
- Novo teste: `Tests/GameplayValidation/CutsceneSkipValidation.cs`.

As cenas já têm a fonte atribuída. Nenhuma configuração manual ou asset externo é necessário. São reutilizadas as transições existentes, incluindo seus fades.

## Validação
11 verificações aprovadas no Unity 6 isolado: clique, Esc, clique repetido, posição no canto inferior direito, texto sem transbordamento, destinos e progresso inicial. Captura final revisada visualmente. Compilação sem erros. O índice de busca do Editor registrou sua exceção conhecida, separada dos testes de gameplay. Resultados e capturas em Documentation/CutsceneSkip. A cena de encerramento recebe um EventSystem com New Input System quando necessário; o aviso antigo de Esc foi substituído pelo botão.

