// Muzik 67 - Interactive Intro Animation, Scroll Reveals & Web Audio Synthesizer

document.addEventListener("DOMContentLoaded", function () {
    initIntroAnimation();
    initScrollReveal();
    initBgmController();
    initScrollVideoController();
});

// =========================================================================
// 1. STORYBOARD INTRO ANIMATION
// =========================================================================
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

// =========================================================================
// 2. SCROLL REVEAL (IntersectionObserver)
// =========================================================================
function initScrollReveal() {
    const revealElements = document.querySelectorAll(".scroll-reveal");
    if (!revealElements || revealElements.length === 0) return;

    if (!("IntersectionObserver" in window)) {
        revealElements.forEach(el => el.classList.add("visible"));
        return;
    }

    const observer = new IntersectionObserver((entries, obs) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add("visible");
                obs.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.15,
        rootMargin: "0px 0px -40px 0px"
    });

    revealElements.forEach(el => observer.observe(el));
}

// =========================================================================
// 3. SCROLL-DRIVEN VIDEO CONTROLLER (Above-water loop + underwater dive on scroll)
// =========================================================================
function initScrollVideoController() {
    const video = document.getElementById("landingBgVideo");
    if (!video) return;

    // Timeline markers in the 28.5s cut video:
    // 0.0s to 9.0s: Above water (lake, clouds, shaders)
    // 9.2s to 11.5s: Submerging down underwater
    // 11.5s to 24.5s: Underwater path with squid and seagrass
    // 25.0s to 28.2s: Rising back up and splashing above water
    const SURFACE_LOOP_START = 0.5;
    const SURFACE_LOOP_END = 9.0;
    const DIVE_START = 9.2;
    const RESURFACE_END = 28.2;

    let targetTime = 0;
    let isTicking = false;

    // When at the top: loop peaceful above-water scene continuously
    video.addEventListener("timeupdate", function () {
        if (window.scrollY <= 60) {
            if (video.currentTime >= SURFACE_LOOP_END || video.currentTime < SURFACE_LOOP_START - 0.2) {
                video.currentTime = SURFACE_LOOP_START;
            }
        }
    });

    function handleScroll() {
        const scrollY = window.scrollY;
        const maxScroll = (document.documentElement.scrollHeight - window.innerHeight) || 1;

        if (scrollY <= 60) {
            // At the top / hero section: ensure video plays naturally in ambient loop
            if (video.paused) {
                video.play().catch(() => {});
            }
            return;
        }

        // As user scrolls down, pause natural play so scroll position drives video
        if (!video.paused) {
            video.pause();
        }

        // Calculate scroll progress from 0% (past hero) to 100% (bottom of page)
        const progress = Math.min(Math.max((scrollY - 60) / (maxScroll - 60), 0), 1);

        // Map progress to the diving -> underwater -> resurfacing timeline
        targetTime = DIVE_START + progress * (RESURFACE_END - DIVE_START);

        if (!isTicking) {
            isTicking = true;
            requestAnimationFrame(smoothUpdate);
        }
    }

    function smoothUpdate() {
        const diff = targetTime - video.currentTime;
        if (Math.abs(diff) > 0.08) {
            // Smooth lerp seeking
            video.currentTime = Math.min(Math.max(video.currentTime + diff * 0.35, 0), 28.3);
            requestAnimationFrame(smoothUpdate);
        } else {
            video.currentTime = targetTime;
            isTicking = false;
        }
    }

    window.addEventListener("scroll", handleScroll, { passive: true });
}

// =========================================================================
// 4. AMBIENT BGM VIDEO AUDIO CONTROLLER
// =========================================================================
function initBgmController() {
    const video = document.getElementById("landingBgVideo");
    const toggleBtn = document.getElementById("bgmToggleBtn");
    const icon = document.getElementById("bgmIcon");
    const label = document.getElementById("bgmLabel");

    if (!video || !toggleBtn) return;

    toggleBtn.addEventListener("click", function () {
        if (video.muted) {
            video.muted = false;
            video.volume = 0.6;
            video.play().catch(err => console.log("Video audio play prevented:", err));
            toggleBtn.classList.add("playing");
            if (icon) icon.className = "bi bi-volume-up-fill text-success";
            if (label) label.textContent = "Ambience: On";
        } else {
            video.muted = true;
            toggleBtn.classList.remove("playing");
            if (icon) icon.className = "bi bi-volume-mute-fill";
            if (label) label.textContent = "Play Ambience";
        }
    });
}

// =========================================================================
// 5. INTERACTIVE PIANO / NOTE SYNTHESIZER (Web Audio API)
// =========================================================================
let pianoAudioCtx = null;

window.playTone = function (freq, btnElement) {
    try {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return;

        if (!pianoAudioCtx) {
            pianoAudioCtx = new AudioContext();
        }
        if (pianoAudioCtx.state === 'suspended') {
            pianoAudioCtx.resume();
        }

        const now = pianoAudioCtx.currentTime;

        const osc1 = pianoAudioCtx.createOscillator();
        const osc2 = pianoAudioCtx.createOscillator();
        const gainNode = pianoAudioCtx.createGain();

        osc1.type = 'triangle';
        osc1.frequency.setValueAtTime(freq, now);

        osc2.type = 'sine';
        osc2.frequency.setValueAtTime(freq * 2, now);

        gainNode.gain.setValueAtTime(0.001, now);
        gainNode.gain.linearRampToValueAtTime(0.25, now + 0.02);
        gainNode.gain.exponentialRampToValueAtTime(0.0001, now + 1.2);

        osc1.connect(gainNode);
        osc2.connect(gainNode);
        gainNode.connect(pianoAudioCtx.destination);

        osc1.start(now);
        osc2.start(now);
        osc1.stop(now + 1.25);
        osc2.stop(now + 1.25);

        if (btnElement) {
            btnElement.classList.add("key-active");
            setTimeout(() => {
                btnElement.classList.remove("key-active");
            }, 180);
        }
    } catch (err) {
        console.error("Synthesizer error:", err);
    }
};
