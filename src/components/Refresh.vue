<template>
<div id="refresher">
  <b-button class="refresh" id="rb" @click="refreshGraph" variant="info">&#8635;</b-button>
  <b-button class="refresh" id="rs" variant="info" disabled>
    <b-spinner small></b-spinner>
    <span class="sr-only">Loading...</span>
  </b-button>
</div>
</template>

<script>
export default {
name: "Refresh",
  methods: {
    displayRefreshLoad (b) { // Used when pushing the Refresh button
      const spin = document.getElementById("rs")
      const button = document.getElementById("rb")
      if (b) {
        spin.style.display = "block"
        button.style.display = "none"
      } else {
        spin.style.display = "none"
        button.style.display = "block"
      }
    },
    refreshGraph () {
      this.$emit('button-click')
    }
  }
}
</script>

<style scoped>
.refresh {
  position: fixed;
  bottom: 70px;
  left: 20px;
}

#rs {
  display: none;
}
</style>
