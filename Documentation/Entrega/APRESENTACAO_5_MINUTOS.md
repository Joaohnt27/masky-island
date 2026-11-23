# Defesa de Masky Island - 5 minutos

## Preparação

Gravar a build Windows entregue, em 1920x1080 ou 1280x720, com voz da equipe e captura do jogo. OBS Studio pode ser usado. Faça um teste de áudio de 20 segundos antes. O vídeo é a defesa dos alunos: este roteiro não substitui a gravação.

Prepare trechos reais de gameplay com antecedência para caber em cinco minutos. Identifique cortes como “trecho avançado da partida”; não simule que uma partida inteira ocorreu em três minutos. Se o professor exigir uma tomada contínua, combine a abertura ao vivo com uma partida avançada já preparada e informe isso. Não prometa save/load: a campanha não salva em disco.

## Roteiro cronometrado (300 segundos)

### 0:00-0:35 | Pitch | Rafael
Mostrar menu e a praia. “Em Masky Island, o jogador acorda depois de uma tempestade sem lembrar quem é. O farol está apagado, e cada pista da ilha recupera uma parte dessa história. Criamos uma aventura de exploração e puzzles em que o objetivo é descobrir a identidade do personagem e devolver a luz à ilha.”

### 0:35-1:10 | Escolhas de design | Rafael
Mostrar diário, diálogo e mapa. “Escolhemos cenários ilustrados e personagens em sprites para construir uma atmosfera acolhedora e misteriosa. A orientação aparece no diário, que risca tarefas concluídas, e nas falas. O mapa só é recebido do NPC e libera viagem rápida para lugares conhecidos. Assim, a interface faz parte da narrativa e evita sobrecarregar a tela.”

### 1:10-2:00 | Exploração e mineração | João
Demonstrar movimento, mochila e uma tentativa de mineração. “Usamos WASD e clique nos objetos. O jogador coleta mochila e picareta antes de minerar. Cada tentativa dura quatro segundos e pode não encontrar um cristal; não é uma coleta instantânea. O inventário recebe os cristais e a animação acompanha o progresso. Ao encaixar as três formas no altar, um esconderijo revela os materiais para a chave.”

### 2:00-2:45 | Fabricação e enigma | João
Mostrar materiais, uma ação de precisão da bancada e a porta externa. “A chave precisa ser fabricada. O jogador prepara os materiais e confirma golpes quando o indicador entra na região correta. No farol, a chave e a interpretação da inscrição são necessárias para abrir a porta. As pistas usam formas já reconhecidas durante a aventura.” Não revelar a senha no pitch público; revelar na defesa técnica se perguntarem.

### 2:45-3:30 | Clímax | Rafael
Mostrar os três ajustes do farol e alguns segundos do encerramento. “O desafio final retoma a linguagem do crafting. São três ajustes de precisão, com feedback de acerto e possibilidade de tentar novamente. Ao terminar, o farol acende, a história se conclui em vídeo e os créditos são exibidos.”

### 3:30-4:25 | Arquitetura e desafios | João
Mostrar brevemente GameManager, estrutura de pastas e README; voltar ao jogo. “O projeto usa Unity 6, C#, URP, New Input System e TextMeshPro. O GameManager mantém inventário, lembranças e desbloqueios entre as cenas. As interfaces reagem às mudanças de estado. Um desafio foi impedir que cristais encaixados desaparecessem ao sair da caverna; resolvemos mantendo a conclusão no estado central e reconstruindo sua representação visual. Também usamos bloqueios de movimento por proprietário para evitar conflitos entre diálogos, puzzles e mineração.”

### 4:25-5:00 | Entrega e fechamento | Ambos
Mostrar os links públicos e a build em execução. “O código está organizado no GitHub, com instruções de compilação e capturas. A versão Windows está disponível no Itch.io. A documentação registra as regras e decisões técnicas. O resultado é uma aventura completa, da chegada à ilha até a restauração do farol.” Só dizer “disponível” após confirmar o link público. Encerrar: “Em Masky Island, encontrar o caminho também significa lembrar quem você é.”

## Perguntas técnicas para ensaiar

1. **Por que 2.5D?** Os cenários ilustrados e sprites dão a aparência 2D; posições, colisores e interação usam o espaço 3D da Unity. Isso aproveita a composição artística sem exigir modelagem completa.
2. **Onde vive o progresso?** Em GameManager, singleton com DontDestroyOnLoad: conjuntos para lembranças, locais e encaixes; dicionário para itens. Cenas não são a fonte de verdade.
3. **O jogo salva?** A campanha persiste entre cenas na mesma sessão, não após fechar. Nome e configurações usam PlayerPrefs. Um save completo seria uma melhoria futura.
4. **Como impedir mineração simultânea?** MiningSpot verifica requisitos e IsMining; uma coroutine atualiza o progresso. O movimento fica bloqueado durante a atividade.
5. **Como funciona a chance?** A cena configura quatro segundos e 35% de descoberta por tentativa concluída. Os parâmetros são serializados; chance não é garantia de encontrar em um número fixo de tentativas.
6. **Por que não há timer global?** O foco passou a ser exploração narrativa e resolução de puzzles. O tempo local do QTE fornece ritmo sem penalizar toda a campanha.
7. **Como funciona o crafting?** O estado da bancada avança pelas etapas; três ações são validadas pela posição do indicador. A coleta da chave conclui a receita; falhas não destroem materiais.
8. **Como impedir entrada antecipada no farol?** GameManager guarda leitura da pista, uso da chave e desbloqueio. A interação e a transição de cena verificam esses requisitos; o mapa aponta para o exterior.
9. **Qual é a lógica da senha?** Quadrado, círculo e triângulo: contar lados retos produz 4, 0 e 3. É necessário ler a pista e usar a chave.
10. **Por que o altar continua preenchido?** Os encaixes são registrados no GameManager e o componente visual consulta esse estado quando a cena retorna.
11. **Como evitar travar o movimento?** Bloqueios identificam o componente proprietário. Ao fechar a interface ou terminar a atividade, o dono libera seu bloqueio sem remover o de outra interação.
12. **Como vocês testaram?** Explicar os relatórios e a compilação realmente executados, distinguindo testes de subsistemas com estado preparado da partida manual completa na build.
13. **Por que Windows no Itch.io?** A entrega atual usa vídeos como VideoClip e foi preparada para desktop; Web exigiria adaptação e testes próprios. Não é apenas mudar o destino da build.
14. **Como foi o versionamento?** O projeto já tinha configuração de Unity Version Control. O repositório GitHub começa na importação real da versão atual; não apresentamos commits retroativos como histórico de desenvolvimento.
15. **O que melhorar depois?** Save de campanha, mais testes com jogadores, revisão de acessibilidade e ajuste das probabilidades após observação de tempo real de jogo.

## Conferência da gravação

- Duração final 5:00 (ajustar a fala no ensaio, não acelerar ilegivelmente).
- Voz audível; volume do jogo abaixo da narração; cursor e texto legíveis.
- Nenhum erro de Console ou aviso do Editor visível: usar a build.
- Links realmente públicos, PDF final e autoria revisados.
- Reproduzir o vídeo exportado do início ao fim antes de enviar.
