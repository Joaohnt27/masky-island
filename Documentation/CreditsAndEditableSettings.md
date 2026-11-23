# Créditos e configurações editáveis

## Editar os créditos

Abra `Assets/Scenes/Credits.unity`. Na Hierarchy, selecione `CreditsCanvas → CreditsViewport → CreditsText`. Edite **Text Input** no componente TextMeshPro. O texto já contém exemplos para equipe, programação, arte/animação, áudio, orientação e agradecimentos. Substitua os campos entre colchetes pelos nomes e atribuições reais. Os exemplos não representam autores ou licenças verificados.

O texto é um objeto salvo na cena: fonte, cor, alinhamento e largura podem ser editados. Ao iniciar, o script calcula a altura conforme o texto e o posiciona abaixo da janela de rolagem. `CreditsViewport` limita a região visível com RectMask2D. Preserve o pivot superior de CreditsText para a rolagem funcionar.

Selecione o objeto `CreditsController` para ajustar:

- **Scroll Speed:** velocidade em unidades de UI por segundo; padrão 45.
- **Initial Delay / Final Delay:** pausas inicial e final; padrões 1 e 2 segundos.
- **Return Automatically:** retorno ao menu após a última linha sair da janela. Desative para aguardar o botão Voltar ou ESC.

O botão Créditos do menu abre a cena com o fade existente. Voltar, ESC e o fim automático retornam diretamente ao menu principal, sem repetir nickname ou vídeo. A preferência PlayerNickname é preservada. A cena já está incluída no build.

## Editar as configurações

Abra ou recarregue `Assets/Scenes/MainMenu.unity` fora do Play Mode. Expanda `MainMenuController → SettingsPanel`. Ative temporariamente o SettingsPanel no Inspector para visualizar a interface na Scene/Game View.

- `SettingsBackground`: fundo exclusivo das configurações, ocupando a tela inteira.
- `SettingsCard`: cartão, título, botões das abas e Voltar.
- Páginas `ÁUDIO`, `VÍDEO`, `CONTROLES`: conteúdo de cada aba. Para editar uma página oculta, ative-a e desative as outras temporariamente.
- Sliders, textos, imagens, botões e templates dos dropdowns são objetos reais e editáveis.

Edite cores nos componentes Image/Button, tipografia nos TextMeshPro e tamanhos/posições nos RectTransform. Ao terminar, deixe apenas a página ÁUDIO ativa e o SettingsPanel desativado. Ao executar, o script garante que o painel comece fechado e abra na aba Áudio.

A aparência não é recriada no início do jogo. O script só liga controles, alterna páginas, preenche opções disponíveis de vídeo e atualiza valores/feedback. O estado selecionado das abas usa a cor Disabled configurável no Button. Preserve as referências serializadas no SettingsMenuUI ao substituir objetos. Não é necessário adicionar callbacks duplicados: os controles internos são conectados uma vez pelo Awake.

## Arquivos e preservação

Criados: `Credits.unity` e `.meta`, `CreditsController.cs` e `.meta`, este guia, `CreditsSettingsValidationResults.txt` e a ferramenta de autoria isolada em `Tests/UIAuthoring/AuthorMenuUI.cs` / `Run.ps1`.

Modificados: `MainMenuController.cs` (abrir/retornar dos créditos), `SettingsMenuUI.cs` (referências salvas e construção restrita ao Editor), `MainMenu.unity` (painel e callback de Créditos), `EditorBuildSettings.asset`, suíte `Tests/GameplayValidation` e documentação do menu.

Na MainMenu, os únicos objetos antigos modificados foram o callback de CreditsButton, os filhos do MainMenuController e as referências de SettingsMenuUI. VideoPlayer, RenderTexture, nickname, layout antigo, Splash, Cave, Island e sistemas de gameplay foram preservados.

Nenhum asset externo ou pacote é necessário. Nenhuma atribuição obrigatória no Inspector está pendente; resta substituir o texto de exemplo e ajustar a aparência desejada. A ferramenta de autoria já foi executada em cópia isolada; não precisa ser executada para editar os objetos.

## Validação

A suíte abre MainMenu no Editor e verifica que SettingsPanel existe salvo antes do Play Mode. Em execução, verifica abertura dos créditos pelo botão, texto editável, rolagem com tempo pausado, retorno pelo botão e retorno automático sem repetir a introdução. Também exercita configurações, fades, missão, moedas, mineração, vitória e derrota.

O teste sem GPU não valida a aparência final, clique físico, tecla ESC física ou reprodução real do vídeo. Confira esses pontos no Editor, especialmente ao alterar o texto, layout e resolução. Resultado da execução em `CreditsSettingsValidationResults.txt`.

Compilação no Unity sem erros; **81 verificações aprovadas (ALL PASSED)**. Cinco cenas passaram na checagem de referências locais e IDs únicos. A exceção conhecida do índice de busca do Editor foi registrada separadamente; nenhuma exceção dos scripts do jogo foi ignorada.

## Fade e background das configurações

Abrir Configurações e usar Voltar/ESC agora reutiliza o fade do MainMenuController: 0,47 segundo para cobrir a tela e 0,47 para revelar o outro painel, sem recarregar a cena. Cliques repetidos e interação com os painéis ficam bloqueados durante a transição. A duração pode ser alterada em MainMenuController → Fade Duration, compartilhada com os demais fades locais do menu.

Para configurar a imagem, fora do Play Mode:

1. Importe a imagem para Assets e configure Texture Type como Sprite (2D and UI), aplicando a alteração.
2. Abra MainMenu e ative temporariamente MainMenuController → SettingsPanel para visualizar.
3. Selecione SettingsPanel → SettingsBackground. Em Image, arraste o sprite para Source Image e defina Color como branco com Alpha 1 para manter as cores originais.
4. Use Image Type Simple. Preserve Aspect evita distorção, mas pode deixar margens conforme a proporção da imagem; desativado, a imagem preenche todo o retângulo.
5. SettingsCard é o cartão sobre o fundo; sua cor/transparência pode ser ajustada separadamente. Desative SettingsPanel ao terminar e salve a cena.

Nenhuma imagem específica foi escolhida porque o usuário deseja configurá-la. O fundo de cor atual continua funcionando até que um sprite seja atribuído. Nenhuma nova referência de script é necessária.

Nesta revisão: MainMenuController.cs passou a oferecer a troca de painéis com seu fade existente; SettingsMenuUI.cs usa essa troca; MainMenu.unity identifica o fundo como SettingsBackground. A suíte e este guia foram atualizados. Não foram criados scripts ou assets adicionais.

Validação desta revisão: 81 verificações aprovadas no Unity, incluindo abertura/fechamento com fade, repetição de comandos e abertura com Time.timeScale zero. Compilação sem erros. A aparência final do background escolhido e dos fades ainda precisa de conferência visual no Editor.
