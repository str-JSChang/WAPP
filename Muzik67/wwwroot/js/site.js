// Muzik 67 - Interactive Intro Animation & Application Script

document.addEventListener("DOMContentLoaded", function () {
    initIntroAnimation();
});

function initIntroAnimation() {
    const overlay = document.getElementById("introOverlay");
    const introContent = document.querySelector(".intro-content");
    const landingWrapper = document.getElementById("landingWrapper");
    const skipBtn = document.getElementById("skipIntroBtn");
    const replayBtns = document.querySelectorAll(".replay-intro-trigger");
    const soundToggleBtn = document.getElementById("soundToggleBtn");

    if (!overlay) return;

    let soundEnabled = true;
    let audioCtx = null;
    let isIntroPlaying = true;
    let timer1 = null;
    let timer2 = null;
    let timer3 = null;

    // Optional audio chime synthesis using Web Audio API
    function playIntroChord() {
        if (!soundEnabled) return;
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) return;
            if (!audioCtx) {
                audioCtx = new AudioContext();
            }
            if (audioCtx.state === 'suspended') {
                audioCtx.resume();
            }

            // Play a cheerful music chime arpeggio: C5 (523.25Hz), E5 (659.25Hz), G5 (783.99Hz), C6 (1046.50Hz)
            const notes = [523.25, 659.25, 783.99, 1046.50];
            const now = audioCtx.currentTime;

            notes.forEach((freq, index) => {
                const osc = audioCtx.createOscillator();
                const gain = audioCtx.createGain();

                osc.type = 'triangle';
                osc.frequency.setValueAtTime(freq, now + index * 0.22);

                gain.gain.setValueAtTime(0.001, now + index * 0.22);
                gain.gain.exponentialRampToValueAtTime(0.18, now + index * 0.22 + 0.05);
                gain.gain.exponentialRampToValueAtTime(0.0001, now + index * 0.22 + 1.2);

                osc.connect(gain);
                gain.connect(audioCtx.destination);

                osc.start(now + index * 0.22);
                osc.stop(now + index * 0.22 + 1.3);
            });
        } catch (e) {
            console.log("Audio notice:", e);
        }
    }

    // Storyboard Animation Sequence
    function startIntroSequence() {
        isIntroPlaying = true;
        clearTimeout(timer1);
        clearTimeout(timer2);
        clearTimeout(timer3);

        // Reset overlay states
        overlay.style.display = "flex";
        overlay.classList.remove("fade-out");
        if (introContent) {
            introContent.classList.remove("logo-fade-out");
        }
        if (landingWrapper) {
            landingWrapper.classList.remove("animate-landing-in");
        }

        // Try playing harmonic chime
        playIntroChord();

        // Slide 1 -> Slide 2: Logo fades out after ~2.4 seconds
        timer1 = setTimeout(function () {
            if (!isIntroPlaying) return;
            if (introContent) {
                introContent.classList.add("logo-fade-out");
            }
        }, 2400);

        // Slide 2 -> Slide 3: Screen fades out, animation shifts into landing page
        timer2 = setTimeout(function () {
            if (!isIntroPlaying) return;
            overlay.classList.add("fade-out");
            if (landingWrapper) {
                landingWrapper.classList.add("animate-landing-in");
            }
        }, 3400);

        // End of intro sequence
        timer3 = setTimeout(function () {
            overlay.style.display = "none";
            isIntroPlaying = false;
        }, 4600);
    }

    function skipIntro() {
        if (!isIntroPlaying && overlay.style.display === "none") return;
        isIntroPlaying = false;
        clearTimeout(timer1);
        clearTimeout(timer2);
        clearTimeout(timer3);

        overlay.classList.add("fade-out");
        if (landingWrapper) {
            landingWrapper.classList.add("animate-landing-in");
        }

        setTimeout(function () {
            overlay.style.display = "none";
        }, 600);
    }

    // Skip button click
    if (skipBtn) {
        skipBtn.addEventListener("click", skipIntro);
    }

    // Keyboard support: Escape or Space skips intro
    document.addEventListener("keydown", function (e) {
        if (isIntroPlaying && (e.key === "Escape" || e.code === "Space")) {
            e.preventDefault();
            skipIntro();
        }
    });

    // Sound toggle button
    if (soundToggleBtn) {
        soundToggleBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            soundEnabled = !soundEnabled;
            soundToggleBtn.textContent = soundEnabled ? "🔊 Sound: On" : "🔇 Sound: Off";
            if (soundEnabled) {
                playIntroChord();
            }
        });
    }

    // Replay intro triggers
    replayBtns.forEach(btn => {
        btn.addEventListener("click", function (e) {
            e.preventDefault();
            window.scrollTo({ top: 0, behavior: "smooth" });
            startIntroSequence();
        });
    });

    // Start on first visit
    startIntroSequence();
}
