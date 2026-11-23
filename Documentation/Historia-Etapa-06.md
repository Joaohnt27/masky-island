# Etapa 06 — restauração do farol e final

## Entrega

O jogo agora possui um caminho de conclusão: menu → conversa inicial → inscrição do céu → mineração e encaixes → bilhete da volta → farol → vitória.

Depois de recuperar as três lembranças, o estranho indica o pequeno farol à esquerda da ilha, perto da costa. Aproxime-se e pressione **E** para abrir o mecanismo. Escolha **USAR MÁSCARA E ACENDER**.

A luz cresce durante três segundos; o personagem fica impedido de andar e não pode iniciar outra ativação. A pausa congela a animação. Ao terminar, o farol permanece aceso e a tela existente de resultado apresenta **A LUZ VOLTOU**, com um encerramento narrado em texto e os botões **RECOMEÇAR** e **MENU PRINCIPAL**.

As três lembranças são necessárias também na validação global do GameManager. Completar o terceiro puzzle apenas libera o farol: não aciona vitória automaticamente. A máscara é a ação narrativa do mecanismo, sem introduzir um quarto item de inventário.

## Arquivos criados

- `Assets/Scripts/LighthouseInteraction.cs` e `.meta`: interação por E, validação do mecanismo, restauração gradual, pausa e cancelamento seguro durante reset/desativação.
- `Assets/Art/Materials/LighthouseStone.mat`, `LighthouseRoof.mat`, `LighthouseFrame.mat`, `LighthouseGlow.mat`, `LighthouseBeam.mat`, `LighthouseBeams.asset` e respectivos `.meta`: materiais e geometria simples do farol/luz.
- `Tests/UIAuthoring/AuthorFinale.cs`: configuração incremental em cópia isolada. Não executar novamente sobre as cenas prontas.
- `Tests/GameplayValidation/FinaleValidation.cs` e `RunFinale.ps1`: percurso automatizado do menu à vitória e validações do final.
- Este relatório, resultados e capturas.

## Arquivos modificados

- `Assets/Scripts/GameManager.cs`: propriedade HasAllMemories e bloqueio da conclusão sem as três lembranças, durante pausa ou transição.
- `Assets/Scripts/MissionResultUI.cs`: título e texto do encerramento; preservados os botões e o vínculo com o estado persistente.
- `Assets/Scripts/DialogueController.cs`: NPC indica onde está o farol e como usar o mecanismo após a terceira lembrança.
- `Assets/Scenes/Island.unity`: farol, efeito de luz, prompt, painel do mecanismo e referências. Ajustado o espaço do texto final.
- `Assets/Scenes/Cave.unity`: ajuste do texto da tela de resultado compartilhada com o mesmo controlador.

## Configuração manual no Unity

Nenhuma ligação adicional necessária. LighthouseMechanism já possui player, prompt, painel, botões e beacon atribuídos. A interação usa a proximidade e a tecla E existentes, sem Rigidbody ou novo sistema de input.

A duração visual pode ser ajustada em **LighthouseMechanism → Restoration Duration** (padrão: 3 segundos). A mineração mantém quatro segundos por tentativa e 35% de chance; os dados de inventário/lembranças continuam no GameManager existente.

## Arte e limitações

O farol usa geometria simples de protótipo, com luz ampliada por código. A partida do barco é contada no texto final; não foi adicionada uma animação de barco, cutscene em vídeo ou arte externa. A animação completa de mineração do personagem continua como acabamento pendente, sem impedir concluir o jogo.

## Validação

37 verificações aprovadas no percurso automatizado: pré-requisitos, três puzzles, extrações, ativação única, crescimento da luz, pausa, bloqueio do inventário, vitória, textos, alcance dos cliques, reinício, reset durante restauração e retorno ao menu. Compilação concluída sem erros de código. A ocorrência conhecida do índice de busca do Editor foi registrada separadamente dos testes de gameplay.

Capturas em 1280 × 720 inspecionadas: LighthouseMechanismPreview.png, LighthouseLightingPreview.png e IslandEndingPreview.png. Resultado detalhado: FinaleValidationResults.txt.

O percurso automatizado começa no menu já disponível para clicar em Jogar (não revalida nickname/vídeo de introdução). Usa a lógica real dos diálogos, botões dos três puzzles, extrações de quatro segundos, transições, mecanismo, vitória e botões finais. Posiciona o personagem nos locais de interação e força chance de mineração em 100% somente durante o teste, para evitar resultados aleatórios instáveis.

Isso valida a progressão funcional, mas não substitui uma sessão manual completa de caminhada, avaliação da dificuldade com 35% de chance ou revisão de outras resoluções. Não há salvamento em disco.

