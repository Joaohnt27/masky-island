# Animação de mineração do personagem

O personagem mantém os pés no chão, prepara a picareta, golpeia e retorna à posição de espera. Há seis poses para a esquerda e seis para a direita, no estilo pintado do jogo. A direção acompanha a rocha selecionada.

## Integração
- `MiningSpot`: disponibiliza o relógio dos golpes e a direção da rocha, sem alterar duração ou probabilidades da mineração.
- `PlayerSpriteAnimation`: reproduz MineLeft/MineRight durante a tentativa; volta ao idle após conclusão ou cancelamento. A pausa congela a animação.
- `MiningWorldFeedback`: oculta a ferramenta flutuante quando a animação completa está disponível; mantém a alternativa para personagens sem os novos clips.
- `GameplayAudioFeedback`: usa o momento de impacto do mesmo ciclo para tocar os golpes.
- `Cave.unity`: referência do ponto de mineração no componente de animação do personagem.
- `Player.controller`: dois estados adicionais, preservando os oito estados de caminhada/repouso.

## Assets novos
- `Assets/Art/Characters/Player/PlayerMining-v1.png`: folha transparente, seis colunas e duas linhas.
- `Assets/Animations/Player/MineLeft.anim` e `MineRight.anim`: seis poses por ciclo.
- Metas dos assets, com recortes e pivôs alinhados pelos pés. A escala usa a altura do corpo na pose de espera, não a altura da picareta levantada.

## Autoria e testes
`Tests/UIAuthoring/AuthorMiningAnimation.cs` importa os sprites e configura a cena na cópia de validação. `AuthorPlayerAnimation` preserva estados extras ao recriar a locomoção. Os validadores antigos de picareta aceitam a ferramenta incorporada ao sprite; o de locomoção conta apenas os estados de caminhada/idle.

Resultados e capturas da execução: nesta pasta. Nenhuma configuração manual necessária após importar os assets no Unity. Nenhum asset de mineração pendente.

## Validação executada
37 verificações aprovadas no Play Mode de Unity 6000.6.0f1: três rochas, seis poses, direção, bloqueio de movimento, pausa, cancelamento, falha sem recompensa, proteção contra cliques repetidos e retorno ao idle. Capturas dos dois perfis revisadas visualmente. O Editor apresentou uma exceção preexistente no índice UnityEditor.Search.SearchDatabase; não houve erros de gameplay. Os assets copiados ao projeto principal tiveram seus hashes conferidos. Backup da cena e controller anteriores em Builds/MiningAnimationBackup.

